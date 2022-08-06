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
// /api/v3/user?id=...
func GetUserHandler(ctx echo.Context) error {
	username := ctx.Param("username")
	id := ctx.QueryParam("id")

	var dbUser db.User
	var err error

	if username != "" {
		dbUser, err = db.DBQueries.GetUserByUsername(db.DBContext, username)
	} else if id != "" {
		dbUser, err = db.DBQueries.GetUserById(db.DBContext, id)
	} else {
		return echo.NewHTTPError(http.StatusNotFound)
	}

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
