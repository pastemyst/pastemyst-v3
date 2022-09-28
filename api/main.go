package main

import (
	"fmt"
	"pastemyst-api/auth"
	"pastemyst-api/changelog"
	"pastemyst-api/config"
	"pastemyst-api/db"
	"pastemyst-api/handlers"
	"pastemyst-api/language"
	"pastemyst-api/logging"
	"pastemyst-api/validation"
	"time"

	"github.com/go-co-op/gocron"
	"github.com/go-playground/validator/v10"
	"github.com/golang-migrate/migrate/v4"
	"github.com/golang-migrate/migrate/v4/database/postgres"
	_ "github.com/golang-migrate/migrate/v4/source/file"
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

	sqldb, err := db.InitDb()
	if err != nil {
		panic(err)
	}

	driver, err := postgres.WithInstance(sqldb, &postgres.Config{})
	if err != nil {
		panic(err)
	}

	// run migrations
	migrations, err := migrate.NewWithDatabaseInstance("file://db/migrations", "postgres", driver)
	if err != nil {
		panic(err)
	}

	err = migrations.Up()
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

	err = language.LoadLanguages()
	if err != nil {
		panic(err)
	}

	auth.InitAuth()

	e := echo.New()

	e.Validator = &validation.CustomValidator{Validator: validator.New()}

	e.Pre(middleware.RemoveTrailingSlash())

	e.Use(middleware.CORSWithConfig(middleware.CORSConfig{
		AllowOrigins:     []string{"*"},
		AllowHeaders:     []string{echo.HeaderOrigin, echo.HeaderAccept, echo.HeaderContentType, echo.HeaderAuthorization},
		AllowCredentials: true,
	}))

	e.Use(session.Middleware(sessions.NewCookieStore([]byte(config.Cfg.Secrets.Session))))

	e.Use(auth.AuthMiddleware)

	e.GET("/api/v3/login/:provider", handlers.LoginHandler)
	e.GET("/api/v3/login/:provider/callback", handlers.CallbackHandler)

	e.POST("/api/v3/auth/register", handlers.PostRegisterHandler)
	e.GET("/api/v3/auth/self", handlers.GetSelfHandler)
	e.GET("/api/v3/auth/logout", handlers.GetLogoutHandler)

	e.GET("/api/v3/meta/version", handlers.GetVersionHandler)
	e.GET("/api/v3/meta/releases", handlers.GetReleasesHandler)
	e.GET("/api/v3/meta/active_pastes", handlers.GetActivePastesHandler)

	e.GET("/api/v3/lang/all", handlers.GetAllLangsHandler)

	e.GET("/api/v3/user", handlers.GetUserHandler)
	e.GET("/api/v3/user/:username", handlers.GetUserHandler)
	e.GET("/api/v3/user/:username/pastes", handlers.GetUserPastesHandler)

	e.GET("/api/v3/paste/:id", handlers.GetPaseHandler)
	e.GET("/api/v3/paste/:id/stats", handlers.GetPasteStatsHandler)
	e.GET("/api/v3/paste/:id/langs", handlers.GetPasteLangStatsHandler)
	e.GET("/api/v3/paste/:id/star", handlers.IsPasteStarredHandler)
	e.POST("/api/v3/paste/:id/star", handlers.StarPasteHandler)
	e.DELETE("/api/v3/paste/:id", handlers.DeletePasteHandler)
	e.POST("/api/v3/paste", handlers.CreatePasteHandler)

	// set cron to delete expired pastes every 5 seconds
	scheduler := gocron.NewScheduler(time.UTC)
	scheduler.Every(5).Seconds().Do(func() {
		start := time.Now()

		deletedCount, err := db.DBQueries.DeleteExpiredPastes(db.DBContext)
		if err != nil {
			logging.Logger.Errorf("Failed to delete expired pastes: %s", err.Error())
			return
		}

		end := time.Now()

		if deletedCount > 0 {
			logging.Logger.Infof("Deleted %d paste(s) in %s", deletedCount, end.Sub(start))
		}
	})
	scheduler.StartAsync()

	fmt.Printf("\nRunning pastemyst version %s\n", changelog.Version)

	e.Logger.Fatal(e.Start(fmt.Sprintf(":%d", config.Cfg.Api.Port)))
}
