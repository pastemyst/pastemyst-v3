package handlers

import (
	"net/http"
	"pastemyst-api/language"

	"github.com/labstack/echo/v4"
)

// Returns the list of all languages.
//
// /api/v3/lang/all
func GetAllLangsHandler(ctx echo.Context) error {
	// let browsers know this resource can be cached for a week max
	ctx.Response().Header().Add("Cache-Control", "max-age=604800")

	return ctx.JSON(http.StatusOK, language.Languages)
}
