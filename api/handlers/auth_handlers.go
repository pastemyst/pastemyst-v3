package handlers

import (
	"encoding/json"
	"fmt"
	"net/http"
	"pastemyst-api/auth"
	"pastemyst-api/config"
	"pastemyst-api/db"
	"pastemyst-api/logging"
	"pastemyst-api/utils"

	"github.com/gorilla/sessions"
	"github.com/labstack/echo-contrib/session"
	"github.com/labstack/echo/v4"
)

func LoginGithubHandler(ctx echo.Context) error {
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

	url := auth.GithubOauthConfig.AuthCodeURL(state)

	return ctx.Redirect(http.StatusTemporaryRedirect, url)
}

func CallbackGithubHandler(ctx echo.Context) error {
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

	token, err := auth.GithubOauthConfig.Exchange(db.DBContext, code)
	if err != nil {
		logging.Logger.Errorf("Code exchange failed: %s", err.Error())
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	httpClient := &http.Client{}

	req, err := http.NewRequest(http.MethodGet, "https://api.github.com/user", nil)
	if err != nil {
		logging.Logger.Errorf("Failed creating http.NewRequest: %s", err.Error())
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	req.Header.Set("Accept", "application/vnd.github.v3+json")
	req.Header.Set("Authorization", fmt.Sprintf("token %s", token.AccessToken))

	res, err := httpClient.Do(req)
	if err != nil {
		logging.Logger.Errorf("Failed getting the GitHub user: %s", err.Error())
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	defer res.Body.Close()

	var ghuser map[string]interface{}
	json.NewDecoder(res.Body).Decode(&ghuser)

	fmt.Println(ghuser)

	return ctx.Redirect(http.StatusTemporaryRedirect, config.Cfg.ClientHost)
}
