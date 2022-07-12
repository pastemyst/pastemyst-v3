package db

import (
	"context"
	"database/sql"
	"fmt"
	"pastemyst-api/config"
)

// All declared DB queries as functions.
var DBQueries *Queries

// DB Context used for queries.
var DBContext context.Context

// Connects to the DB.
func InitDb(cfg config.Config) error {
	DBContext = context.Background()

	pdb, err := sql.Open("postgres", fmt.Sprintf("host=%s user=%s password=%s dbname=%s port=%s sslmode=disable", cfg.DbHost, cfg.DbUser, cfg.DbPassword, cfg.DbName, cfg.DbPort))
	if err != nil {
		return err
	}

	DBQueries = New(pdb)

	return nil
}
