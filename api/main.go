package main

import (
	"fmt"
	"pastemyst-api/auth"
	"pastemyst-api/changelog"
	"pastemyst-api/config"
	"pastemyst-api/db"
	"pastemyst-api/handlers"
	"pastemyst-api/logging"
	"pastemyst-api/validation"

	"github.com/go-playground/validator/v10"
	"github.com/gorilla/sessions"
	"github.com/labstack/echo-contrib/session"
	"github.com/labstack/echo/v4"
	_ "github.com/lib/pq"
)

func main() {
	err := config.LoadConfig()
	if err != nil {
		panic(err)
	}

	err = db.InitDb()
	if err != nil {
		panic(err)
	}

	err = logging.InitLogger()
	if err != nil {
		panic(err)
	}
	defer logging.CloseLogger()

	err = changelog.InitChangelog()
	if err != nil {
		panic(err)
	}

	auth.InitAuth()

	e := echo.New()

	e.Validator = &validation.CustomValidator{Validator: validator.New()}

	// TODO: use proper secrets
	e.Use(session.Middleware(sessions.NewCookieStore([]byte("secret"))))

	e.GET("/api/v3/login/github", handlers.LoginGithubHandler)
	e.GET("/api/v3/login/github-callback", handlers.CallbackGithubHandler)

	e.GET("/api/v3/meta/version", handlers.GetVersionHandler)
	e.GET("/api/v3/meta/releases", handlers.GetReleasesHandler)
	e.GET("/api/v3/meta/activePastes", handlers.GetActivePastesHandler)

	e.GET("/api/v3/paste/:id", handlers.GetPaseHandler)
	e.POST("/api/v3/paste/", handlers.CreatePasteHandler)

	fmt.Printf("\nRunning pastemyst version %s\n", changelog.Version)

	e.Logger.Fatal(e.Start(":5000"))
}
