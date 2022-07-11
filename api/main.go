package main

import (
	"context"
	"database/sql"
	"fmt"
	"pastemyst-api/config"
	"pastemyst-api/db"

	_ "github.com/lib/pq"
)

func main() {
	ctx := context.Background()

	cfg, err := config.LoadConfig()
	if err != nil {
		panic(err)
	}

	dsn := fmt.Sprintf("host=%s user=%s password=%s dbname=%s port=%s", cfg.DbHost, cfg.DbUser, cfg.DbPassword, cfg.DbName, cfg.DbPort)
	pdb, err := sql.Open("postgres", dsn)
	if err != nil {
		panic(err)
	}

	queries := db.New(pdb)

	paste, err := queries.GetPaste(ctx, "1234567")
	if err != nil {
		panic(err)
	}

	fmt.Println(paste)

	pasties, err := queries.GetPastePasties(ctx, paste.ID)
	if err != nil {
		panic(err)
	}

	fmt.Println(pasties)

	// e := echo.New()

	// e.GET("/ping", func(ctx echo.Context) error {
	// 	return ctx.String(http.StatusOK, "pong")
	// })

	// e.Logger.Fatal(e.Start(":5000"))
}
