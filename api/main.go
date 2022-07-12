package main

import (
	"pastemyst-api/config"
	"pastemyst-api/db"
	"pastemyst-api/handlers"

	"github.com/labstack/echo/v4"
	_ "github.com/lib/pq"
)

func main() {
	// ctx := context.Background()

	cfg, err := config.LoadConfig()
	if err != nil {
		panic(err)
	}

	err = db.InitDb(cfg)
	if err != nil {
		panic(err)
	}

	e := echo.New()

	e.GET("/api/v3/paste/:id", handlers.GetPaseHandler)
	e.POST("/api/v3/paste/", handlers.CreatePasteHandler)

	e.Logger.Fatal(e.Start(":5000"))
}
