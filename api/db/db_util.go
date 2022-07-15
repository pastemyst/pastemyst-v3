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
func InitDb() error {
	DBContext = context.Background()

	pdb, err := sql.Open("postgres", fmt.Sprintf("host=%s user=%s password=%s dbname=%s port=%s sslmode=disable",
		config.Cfg.DbHost, config.Cfg.DbUser, config.Cfg.DbPassword, config.Cfg.DbName, config.Cfg.DbPort))

	if err != nil {
		return err
	}

	DBQueries = New(pdb)

	return nil
}
