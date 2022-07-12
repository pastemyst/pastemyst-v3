package main

import (
	"fmt"
	"pastemyst-api/changelog"
	"pastemyst-api/config"
	"pastemyst-api/db"
	"pastemyst-api/handlers"
	"pastemyst-api/logging"
	"pastemyst-api/validation"

	"github.com/go-playground/validator/v10"
	"github.com/labstack/echo/v4"
	_ "github.com/lib/pq"
)

func main() {
	cfg, err := config.LoadConfig()
	if err != nil {
		panic(err)
	}

	err = db.InitDb(cfg)
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

	e := echo.New()

	e.Validator = &validation.CustomValidator{Validator: validator.New()}

	fmt.Printf("\nRunning pastemyst version %s\n", changelog.Version)

	e.GET("/api/v3/meta/version", handlers.GetVersionHandler)
	e.GET("/api/v3/meta/releases", handlers.GetReleasesHandler)
	e.GET("/api/v3/meta/activePastes", handlers.GetActivePastesHandler)

	e.GET("/api/v3/paste/:id", handlers.GetPaseHandler)
	e.POST("/api/v3/paste/", handlers.CreatePasteHandler)

	e.Logger.Fatal(e.Start(":5000"))
}
