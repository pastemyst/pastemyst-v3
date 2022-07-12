package handlers

import (
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

	paste := models.Paste{
		Id:        randomPasteId(),
		CreatedAt: time.Now(),
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
