package handlers

import (
	"net/http"
	"pastemyst-api/language"

	"github.com/labstack/echo/v4"
)

// Returns the list of all languages.
//
// /api/v3/lang/all
func GetAllLangs(ctx echo.Context) error {
	return ctx.JSON(http.StatusOK, language.Languages)
}
