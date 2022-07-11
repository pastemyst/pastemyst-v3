package handlers

import (
	"net/http"
	"pastemyst-api/db"
	"pastemyst-api/models"

	"github.com/labstack/echo/v4"
)

func GetPaseHandler(ctx echo.Context) error {
	id := ctx.Param("id")

	dbPaste, err := db.DBQueries.GetPaste(db.DBContext, id)
	if err != nil {
		return ctx.NoContent(http.StatusNotFound)
	}

	dbPasties, err := db.DBQueries.GetPastePasties(db.DBContext, dbPaste.ID)
	if err != nil {
		ctx.Logger().Error("Tried to get all pasties of a paste, but got error.")
		ctx.Logger().Error(err)
		return ctx.NoContent(http.StatusInternalServerError)
	}

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
