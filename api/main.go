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
	"github.com/labstack/echo/v4/middleware"
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

	e.Use(middleware.CORSWithConfig(middleware.CORSConfig{
		AllowOrigins:     []string{"*"},
		AllowHeaders:     []string{echo.HeaderOrigin, echo.HeaderAccept, echo.HeaderContentType, echo.HeaderAuthorization},
		AllowCredentials: true,
	}))

	// TODO: use proper secrets
	e.Use(session.Middleware(sessions.NewCookieStore([]byte(config.Cfg.SessionSecret))))

	e.GET("/api/v3/login/:provider", handlers.LoginHandler)
	e.GET("/api/v3/login/:provider/callback", handlers.CallbackHandler)

	e.POST("/api/v3/auth/register", handlers.PostRegisterHandler)
	e.GET("/api/v3/auth/self", handlers.GetSelfHandler)

	e.GET("/api/v3/meta/version", handlers.GetVersionHandler)
	e.GET("/api/v3/meta/releases", handlers.GetReleasesHandler)
	e.GET("/api/v3/meta/active_pastes", handlers.GetActivePastesHandler)

	e.GET("/api/v3/user/:username", handlers.GetUserHandler)

	e.GET("/api/v3/paste/:id", handlers.GetPaseHandler)
	e.POST("/api/v3/paste/", handlers.CreatePasteHandler)

	fmt.Printf("\nRunning pastemyst version %s\n", changelog.Version)

	e.Logger.Fatal(e.Start(":5000"))
}
