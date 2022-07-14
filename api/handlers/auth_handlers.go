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
	"time"

	"github.com/golang-jwt/jwt/v4"
	"github.com/gorilla/sessions"
	"github.com/labstack/echo-contrib/session"
	"github.com/labstack/echo/v4"
)

// Initiates the OAuth2 flow.
//
// /api/v3/login/:provider
func LoginHandler(ctx echo.Context) error {
	state := utils.RandomId()

	session, err := session.Get("pastemyst_oauth_state", ctx)
	if err != nil {
		logging.Logger.Error("Failed getting the session while starting the OAuth flow.")
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	session.Options = &sessions.Options{
		Path:     "/",
		MaxAge:   86400 * 7,
		HttpOnly: true,
	}

	session.Values["state"] = state

	session.Save(ctx.Request(), ctx.Response())

	url := auth.OAuthProviders[ctx.Param("provider")].AuthCodeURL(state)

	return ctx.Redirect(http.StatusTemporaryRedirect, url)
}

// OAuth2 callback handler.
//
// /api/v3/login/:provider/callback
func CallbackHandler(ctx echo.Context) error {
	session, err := session.Get("pastemyst_oauth_state", ctx)
	if err != nil {
		logging.Logger.Error("Failed getting the session while handling the OAuth callback.")
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	sessionState := session.Values["state"]

	session.Options.MaxAge = 0

	state := ctx.FormValue("state")
	code := ctx.FormValue("code")

	if state != sessionState {
		logging.Logger.Error("Someone tried to do OAuth login but the states did not match.")
		return echo.NewHTTPError(http.StatusBadRequest)
	}

	provider := auth.OAuthProviders[ctx.Param("provider")]

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

	jwtToken := jwt.New(jwt.SigningMethodHS512)

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

		tokenString, err := jwtToken.SignedString([]byte(config.Cfg.JwtSecret))
		if err != nil {
			logging.Logger.Errorf("Failed to sign JWT token: %s", err.Error())
		}

		cookie.Expires = expirationTime
		cookie.Name = "pastemyst"
		cookie.Value = tokenString

		ctx.SetCookie(cookie)

		return ctx.Redirect(http.StatusTemporaryRedirect, fmt.Sprintf("%s/handle-login", config.Cfg.ClientHost))
	} else {
		expirationTime := time.Now().Add(1 * time.Hour)

		jwtToken.Claims = &auth.RegistrationClaims{
			RegisteredClaims: jwt.RegisteredClaims{
				ExpiresAt: jwt.NewNumericDate(expirationTime),
			},
			ProviderId:   providerUser.Id,
			ProviderName: provider.Name,
			AvatarUrl:    providerUser.AvatarUrl,
		}

		tokenString, err := jwtToken.SignedString([]byte(config.Cfg.JwtSecret))
		if err != nil {
			logging.Logger.Errorf("Failed to sign JWT token: %s", err.Error())
		}

		cookie.Expires = expirationTime
		cookie.Name = "pastemyst-registration"
		cookie.Value = tokenString

		ctx.SetCookie(cookie)

		return ctx.Redirect(http.StatusTemporaryRedirect, fmt.Sprintf("%s/create-account?username=%s", config.Cfg.ClientHost, url.QueryEscape(providerUser.Username)))
	}
}
