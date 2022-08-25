package handlers

import (
	"fmt"
	"net/http"
	"net/url"
	"pastemyst-api/auth"
	"pastemyst-api/config"
	"pastemyst-api/db"
	"pastemyst-api/logging"
	"pastemyst-api/utils"
	"pastemyst-api/validation"
	"time"

	"github.com/go-playground/validator/v10"
	"github.com/golang-jwt/jwt/v4"
	"github.com/gorilla/sessions"
	"github.com/labstack/echo-contrib/session"
	"github.com/labstack/echo/v4"
)

// Struct used for receiving the username when creating an account.
type registerData struct {
	Username string `json:"username"`
}

// Initiates the OAuth2 flow.
//
// /api/v3/login/:provider
func LoginHandler(ctx echo.Context) error {
	state := utils.RandomId()

	// short lived session to persist the state used for OAuth
	session, err := session.Get("pastemyst_oauth_state", ctx)
	if err != nil {
		logging.Logger.Error("Failed starting the session while starting the OAuth flow.")
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	session.Options = &sessions.Options{
		Path:     "/",
		MaxAge:   300,
		HttpOnly: true,
	}

	session.Values["state"] = state

	session.Save(ctx.Request(), ctx.Response())

	// get the oauth url for the code and redirect to it
	url := auth.OAuthProviders[ctx.Param("provider")].AuthCodeURL(state)

	return ctx.Redirect(http.StatusTemporaryRedirect, url)
}

// OAuth2 callback handler.
//
// /api/v3/login/:provider/callback
func CallbackHandler(ctx echo.Context) error {
	ses, err := session.Get("pastemyst_oauth_state", ctx)
	if err != nil {
		logging.Logger.Error("Failed getting the session while handling the OAuth callback.")
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	// get the session, and set the maxage to -1 to delete it
	sessionState := ses.Values["state"]
	ses.Options.MaxAge = -1
	ses.Save(ctx.Request(), ctx.Response())

	state := ctx.FormValue("state")
	code := ctx.FormValue("code")

	if state != sessionState {
		logging.Logger.Error("Someone tried to do OAuth login but the states did not match.")
		return echo.NewHTTPError(http.StatusBadRequest)
	}

	provider := auth.OAuthProviders[ctx.Param("provider")]

	// get the access token from the provider
	token, err := provider.Exchange(db.DBContext, code)
	if err != nil {
		logging.Logger.Errorf("Code exchange failed: %s", err.Error())
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	providerUser, err := auth.GetProviderUser(provider, token.AccessToken)
	if err != nil {
		return err
	}

	exists, err := db.DBQueries.ExistsUserByProvider(db.DBContext, db.ExistsUserByProviderParams{
		ProviderName: provider.Name,
		ProviderID:   providerUser.Id,
	})
	if err != nil {
		logging.Logger.Errorf("Failed to check if user exists by provider: %s", err.Error())
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	cookie := &http.Cookie{}
	cookie.Path = "/"
	cookie.HttpOnly = true
	cookie.SameSite = http.SameSiteStrictMode
	cookie.Secure = config.Cfg.Api.Https

	jwtToken := jwt.New(jwt.SigningMethodHS512)

	// if the user already exists, create a jwt cookie and return it, redirect back to homepage
	if exists {
		expirationTime := time.Now().Add(30 * 24 * time.Hour)

		user, err := db.DBQueries.GetUserByProvider(db.DBContext, db.GetUserByProviderParams{
			ProviderName: provider.Name,
			ProviderID:   providerUser.Id,
		})
		if err != nil {
			logging.Logger.Errorf("Failed to get user by provider: %s", err.Error())
			return echo.NewHTTPError(http.StatusInternalServerError)
		}

		jwtToken.Claims = &auth.Claims{
			RegisteredClaims: jwt.RegisteredClaims{
				ExpiresAt: jwt.NewNumericDate(expirationTime),
			},
			Id:       user.ID,
			Username: user.Username,
		}

		tokenString, err := jwtToken.SignedString([]byte(config.Cfg.Secrets.Jwt))
		if err != nil {
			logging.Logger.Errorf("Failed to sign JWT token: %s", err.Error())
			return echo.NewHTTPError(http.StatusInternalServerError)
		}

		cookie.Expires = expirationTime
		cookie.Name = "pastemyst"
		cookie.Value = tokenString

		ctx.SetCookie(cookie)

		return ctx.Redirect(http.StatusTemporaryRedirect, config.Cfg.Client.Host)
	} else {
		// if user doesn't exist yet, redirect to the create account page with the temporary cookie
		expirationTime := time.Now().Add(1 * time.Hour)

		jwtToken.Claims = &auth.RegistrationClaims{
			RegisteredClaims: jwt.RegisteredClaims{
				ExpiresAt: jwt.NewNumericDate(expirationTime),
			},
			ProviderId:   providerUser.Id,
			ProviderName: provider.Name,
			AvatarUrl:    providerUser.AvatarUrl,
		}

		tokenString, err := jwtToken.SignedString([]byte(config.Cfg.Secrets.Jwt))
		if err != nil {
			logging.Logger.Errorf("Failed to sign JWT token: %s", err.Error())
		}

		cookie.Expires = expirationTime
		cookie.Name = "pastemyst-registration"
		cookie.Value = tokenString

		ctx.SetCookie(cookie)

		return ctx.Redirect(http.StatusTemporaryRedirect, fmt.Sprintf("%s/create-account?username=%s", config.Cfg.Client.Host, url.QueryEscape(providerUser.Username)))
	}
}

// Creates a new account.
//
// /api/v3/auth/register
func PostRegisterHandler(ctx echo.Context) error {
	cookie, err := ctx.Cookie("pastemyst-registration")
	if err != nil {
		logging.Logger.Errorf("Trying to register but couldn't get registration cookie: %s", err.Error())
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	data := registerData{}
	if err := ctx.Bind(&data); err != nil {
		logging.Logger.Errorf("Failed to bind data: %s", err.Error())
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	username := data.Username
	if username == "" {
		return echo.NewHTTPError(http.StatusBadRequest, "Missing username.")
	}

	validate := validator.New()
	err = validate.Var(username, "required,max=20,containsany=abcdefghijklmnopqrstuvwxyz0123456789.-_")
	if err != nil {
		return echo.NewHTTPError(http.StatusBadRequest, validation.ValidationErrToMsg(err))
	}

	exists, err := db.DBQueries.ExistsUserByUsername(db.DBContext, username)
	if err != nil {
		logging.Logger.Errorf("Failed to check if user exists by username: %s", err.Error())
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	if exists {
		return echo.NewHTTPError(http.StatusBadRequest, "Username is taken.")
	}

	jwtClaims := &auth.RegistrationClaims{}
	jwtToken, err := jwt.ParseWithClaims(cookie.Value, jwtClaims, func(t *jwt.Token) (interface{}, error) {
		return []byte(config.Cfg.Secrets.Jwt), nil
	})
	if err != nil {
		logging.Logger.Errorf("User tried to create an account with an invalid JWT token: %s", err.Error())
		return echo.NewHTTPError(http.StatusUnauthorized, "You tried to authorize with an invalid JWT token.")
	}

	if !jwtToken.Valid {
		logging.Logger.Errorf("User tried to create an account with an invalid JWT token: %s", err.Error())
		return echo.NewHTTPError(http.StatusUnauthorized, "You tried to authorize with an invalid JWT token.")
	}

	existsByProvider, err := db.DBQueries.ExistsUserByProvider(db.DBContext, db.ExistsUserByProviderParams{
		ProviderName: jwtClaims.ProviderName,
		ProviderID:   jwtClaims.ProviderId,
	})
	if err != nil {
		logging.Logger.Errorf("Failed checking is user exists by provider: %s", err.Error())
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	if existsByProvider {
		return echo.NewHTTPError(http.StatusBadRequest, "A user already exists with the same provider.")
	}

	id := randomUserId()

	_, err = db.DBQueries.CreateUser(db.DBContext, db.CreateUserParams{
		ID:           id,
		CreatedAt:    time.Now().UTC(),
		Username:     username,
		AvatarUrl:    jwtClaims.AvatarUrl,
		ProviderName: jwtClaims.ProviderName,
		ProviderID:   jwtClaims.ProviderId,
	})
	if err != nil {
		logging.Logger.Errorf("Failed to insert user into DB: %s", err.Error())
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	cookie.Expires = time.Unix(0, 0)
	cookie.Name = "pastemyst-registration"
	cookie.Value = ""
	cookie.Path = "/"
	cookie.HttpOnly = true
	cookie.SameSite = http.SameSiteStrictMode
	cookie.Secure = config.Cfg.Api.Https
	ctx.SetCookie(cookie)

	expirationTime := time.Now().Add(30 * 24 * time.Hour)

	newJwt := jwt.New(jwt.SigningMethodHS512)
	newJwt.Claims = &auth.Claims{
		RegisteredClaims: jwt.RegisteredClaims{
			ExpiresAt: jwt.NewNumericDate(expirationTime),
		},
		Id:       id,
		Username: username,
	}

	tokenString, err := newJwt.SignedString([]byte(config.Cfg.Secrets.Jwt))
	if err != nil {
		logging.Logger.Errorf("Failed to sign JWT token: %s", err.Error())
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	newCookie := &http.Cookie{}
	newCookie.Path = "/"
	newCookie.HttpOnly = true
	newCookie.SameSite = http.SameSiteStrictMode
	newCookie.Expires = expirationTime
	newCookie.Name = "pastemyst"
	newCookie.Value = tokenString
	newCookie.Secure = config.Cfg.Api.Https

	ctx.SetCookie(newCookie)

	return ctx.NoContent(http.StatusOK)
}

// Returns the currently authorized user from the provided token.
//
// /api/v3/auth/self
func GetSelfHandler(ctx echo.Context) error {
	user := ctx.Get("user")

	if user == nil {
		return echo.NewHTTPError(http.StatusUnauthorized)
	}

	return ctx.JSON(http.StatusOK, user)
}

// Logs the user out
//
// /api/v3/auth/logout
func GetLogoutHandler(ctx echo.Context) error {
	cookie, err := ctx.Cookie("pastemyst")
	if err != nil {
		return echo.NewHTTPError(http.StatusUnauthorized)
	}

	cookie.Value = ""
	cookie.Expires = time.Unix(0, 0)
	cookie.Path = "/"
	cookie.HttpOnly = true
	cookie.SameSite = http.SameSiteStrictMode
	cookie.Name = "pastemyst"
	cookie.Secure = config.Cfg.Api.Https

	ctx.SetCookie(cookie)

	return ctx.Redirect(http.StatusTemporaryRedirect, config.Cfg.Client.Host)
}

// Generates a random user ID, making sure that it's unique.
func randomUserId() string {
	return utils.RandomIdWhile(func(id string) bool {
		exists, _ := db.DBQueries.ExistsUserById(db.DBContext, id)
		return exists
	})
}
