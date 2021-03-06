package handlers

import (
	"database/sql"
	"net/http"
	"pastemyst-api/db"
	"pastemyst-api/models"
	"pastemyst-api/utils"
	"time"

	"github.com/labstack/echo/v4"
)

// Gets a single paste.
//
// /api/v3/paste/:id
func GetPaseHandler(ctx echo.Context) error {
	id := ctx.Param("id")

	// get paste from db
	dbPaste, err := db.DBQueries.GetPaste(db.DBContext, id)
	if err != nil {
		return ctx.NoContent(http.StatusNotFound)
	}

	// get all pasties tied to this paste from db
	dbPasties, err := db.DBQueries.GetPastePasties(db.DBContext, dbPaste.ID)
	if err != nil {
		ctx.Logger().Error("Tried to get all pasties of a paste, but got error.")
		ctx.Logger().Error(err)
		return ctx.NoContent(http.StatusInternalServerError)
	}

	// create proper models that can be returned
	pasties := make([]models.Pasty, len(dbPasties))
	for i := 0; i < len(dbPasties); i++ {
		pasties[i] = models.Pasty{
			Id:      dbPasties[i].ID,
			Title:   dbPasties[i].Title,
			Content: dbPasties[i].Content,
		}
	}

	paste := models.Paste{
		Id:        dbPaste.ID,
		CreatedAt: dbPaste.CreatedAt,
		ExpiresIn: models.ExpiresIn(dbPaste.ExpiresIn),
		DeletesAt: dbPaste.DeletesAt.Time,
		Title:     dbPaste.Title,
		Pasties:   pasties,
	}

	return ctx.JSON(http.StatusOK, paste)
}

// Creates a new paste.
//
// /api/v3/paste/
func CreatePasteHandler(ctx echo.Context) error {
	var createInfo models.PasteCreateInfo
	if err := ctx.Bind(&createInfo); err != nil {
		return err
	}

	// validate paste
	if err := ctx.Validate(createInfo); err != nil {
		return err
	}

	if createInfo.ExpiresIn == "" {
		createInfo.ExpiresIn = models.ExpiresInNever
	}

	now := time.Now().UTC()

	paste := models.Paste{
		Id:        randomPasteId(),
		CreatedAt: now,
		ExpiresIn: createInfo.ExpiresIn,
		DeletesAt: expiresInToTime(now, createInfo.ExpiresIn),
		Title:     createInfo.Title,
	}

	// create pasties
	pasties := make([]models.Pasty, 0, len(createInfo.Pasties))
	for _, pasty := range createInfo.Pasties {
		pasties = append(pasties, models.Pasty{
			Id:      randomPastyId(pasties),
			Title:   pasty.Title,
			Content: pasty.Content,
		})
	}
	paste.Pasties = pasties

	// insert the paste into the DB
	_, err := db.DBQueries.CreatePaste(db.DBContext, db.CreatePasteParams{
		ID:        paste.Id,
		CreatedAt: time.Now(),
		ExpiresIn: db.ExpiresIn(createInfo.ExpiresIn),
		DeletesAt: sql.NullTime{Time: paste.DeletesAt, Valid: createInfo.ExpiresIn != models.ExpiresInNever},
		Title:     createInfo.Title,
	})
	if err != nil {
		ctx.Logger().Error("Tried to insert a paste into the DB, got error.")
		ctx.Logger().Error(err)
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	// insert pasties into the DB
	for _, pasty := range pasties {
		_, err := db.DBQueries.CreatePasty(db.DBContext, db.CreatePastyParams{
			ID:      pasty.Id,
			PasteID: paste.Id,
			Title:   pasty.Title,
			Content: pasty.Content,
		})
		if err != nil {
			ctx.Logger().Error("Tried to insert a pasty into the DB, got error.")
			ctx.Logger().Error(err)
			return echo.NewHTTPError(http.StatusInternalServerError)
		}
	}

	return ctx.JSON(http.StatusOK, paste)
}

// Generates a random paste ID, making sure that it's unique.
func randomPasteId() string {
	return utils.RandomIdWhile(func(id string) bool {
		exists, _ := db.DBQueries.ExistsPaste(db.DBContext, id)
		return exists
	})
}

// Generates a random pasty ID, making sure that it's unique (inside a single paste).
func randomPastyId(pasties []models.Pasty) string {
	return utils.RandomIdWhile(func(id string) bool {
		for _, pasty := range pasties {
			if pasty.Id == id {
				return true
			}
		}

		return false
	})
}

// Converts the current time and the expires in value to an exact date when the paste should expire.
func expiresInToTime(start time.Time, expiresIn models.ExpiresIn) time.Time {
	switch expiresIn {
	case models.ExpiresInNever:
		return time.Time{}
	case models.ExpiresIn1h:
		return start.Add(time.Hour)
	case models.ExpiresIn2h:
		return start.Add(2 * time.Hour)
	case models.ExpiresIn10h:
		return start.Add(10 * time.Hour)
	case models.ExpiresIn1d:
		return start.AddDate(0, 0, 1)
	case models.ExpiresIn2d:
		return start.AddDate(0, 0, 2)
	case models.ExpiresIn1w:
		return start.AddDate(0, 0, 7)
	case models.ExpiresIn1m:
		return start.AddDate(0, 1, 0)
	case models.ExpiresIn1y:
		return start.AddDate(1, 0, 0)
	}

	return time.Time{}
}
