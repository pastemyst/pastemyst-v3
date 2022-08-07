package handlers

import (
	"database/sql"
	"math"
	"net/http"
	"pastemyst-api/db"
	"pastemyst-api/logging"
	"pastemyst-api/models"
	"strconv"

	"github.com/labstack/echo/v4"
)

// Returns the user with the provided username or id.
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

// Returns the list of all pastes of a user.
// If the user that is logged in is the same as the one that the list of pastes is fetched for, it returns all pastes,
// otherwise it only returns the public pastes.
//
// /api/v3/user/:username/pastes?page=0&page_size=15
func GetUserPastesHandler(ctx echo.Context) error {
	username := ctx.Param("username")
	currentUser, hasCurrentUser := ctx.Get("user").(models.User)

	page, _ := strconv.Atoi(ctx.QueryParam("page"))
	pageSize, _ := strconv.Atoi(ctx.QueryParam("page_size"))

	if pageSize == 0 {
		pageSize = 15
	}

	dbUser, err := db.DBQueries.GetUserByUsername(db.DBContext, username)
	if err != nil {
		return echo.NewHTTPError(http.StatusNotFound)
	}

	var dbPastes []db.Paste
	var totalPastes int64

	if hasCurrentUser && currentUser.Id == dbUser.ID {
		dbPastes, err = db.DBQueries.GetUserAllPastes(db.DBContext, db.GetUserAllPastesParams{
			OwnerID: sql.NullString{String: dbUser.ID, Valid: true},
			Limit:   int32(pageSize),
			Offset:  int32(page * pageSize),
		})

		totalPastes, _ = db.DBQueries.GetUserAllPastesCount(db.DBContext, sql.NullString{String: dbUser.ID, Valid: true})
	} else {
		dbPastes, err = db.DBQueries.GetUserPublicPastes(db.DBContext, db.GetUserPublicPastesParams{
			OwnerID: sql.NullString{String: dbUser.ID, Valid: true},
			Limit:   int32(pageSize),
			Offset:  int32(page * pageSize),
		})

		totalPastes, _ = db.DBQueries.GetUserPublicPastesCount(db.DBContext, sql.NullString{String: dbUser.ID, Valid: true})
	}

	pastes := make([]models.Paste, 0)

	if err != nil {
		return ctx.JSON(http.StatusOK, pastes)
	}

	for _, dbPaste := range dbPastes {
		dbPasties, err := db.DBQueries.GetPastePasties(db.DBContext, dbPaste.ID)
		if err != nil {
			logging.Logger.Errorf("Tried to get all pasties of a paste, but got error: %s", err)
			return echo.NewHTTPError(http.StatusInternalServerError)
		}

		pasties := make([]models.Pasty, 0, len(dbPasties))

		for _, dbPasty := range dbPasties {
			pasties = append(pasties, models.Pasty{
				Id:       dbPasty.ID,
				Title:    dbPasty.Title,
				Content:  dbPasty.Content,
				Language: dbPasty.Language,
			})
		}

		pastes = append(pastes, models.Paste{
			Id:        dbPaste.ID,
			CreatedAt: dbPaste.CreatedAt,
			ExpiresIn: models.ExpiresIn(dbPaste.ExpiresIn),
			DeletesAt: dbPaste.DeletesAt.Time,
			Title:     dbPaste.Title,
			Pasties:   pasties,
			OwnerId:   dbPaste.OwnerID.String,
			Private:   dbPaste.Private,
		})
	}

	totalPages := int64(math.Ceil(float64(totalPastes) / float64(pageSize)))

	res := models.Page[models.Paste]{
		Items:       pastes,
		TotalPages:  totalPages,
		Page:        int64(page),
		PageSize:    int64(pageSize),
		HasNextPage: page < int(totalPages-1),
	}

	return ctx.JSON(http.StatusOK, res)
}
