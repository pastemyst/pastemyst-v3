package handlers

import (
	"net/http"
	"pastemyst-api/db"
	"pastemyst-api/models"

	"github.com/labstack/echo/v4"
)

// Returns the user with the provided username.
//
// /api/v3/user/:username
func GetUserHandler(ctx echo.Context) error {
	username := ctx.Param("username")

	dbUser, err := db.DBQueries.GetUserByUsername(db.DBContext, username)
	if err != nil {
		return echo.NewHTTPError(http.StatusNotFound)
	}

	user := models.User{
		Id:          dbUser.ID,
		CreatedAt:   dbUser.CreatedAt,
		Username:    dbUser.Username,
		AvatarUrl:   dbUser.AvatarUrl,
		Contributor: dbUser.Contributor,
		Supporter:   uint32(dbUser.Supporter),
	}

	return ctx.JSON(http.StatusOK, user)
}
