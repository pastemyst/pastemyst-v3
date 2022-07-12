package handlers

import (
	"net/http"
	"pastemyst-api/changelog"
	"pastemyst-api/db"
	"pastemyst-api/logging"

	"github.com/labstack/echo/v4"
)

type versionResult struct {
	Version string `json:"version"`
}

type releasesResult struct {
	Releases []changelog.Release `json:"releases"`
}

type activePastesResult struct {
	Count int64 `json:"count"`
}

// Get the current version of the app. Based on git tags.
//
// /api/v3/meta/version
func GetVersionHandler(ctx echo.Context) error {
	return ctx.JSON(http.StatusOK, versionResult{Version: changelog.Version})
}

// Get the list of all app releases.
//
// /api/v3/meta/releases
func GetReleasesHandler(ctx echo.Context) error {
	return ctx.JSON(http.StatusOK, releasesResult{Releases: changelog.Releases})
}

// Get the number of currently active (existing) pastes.
//
// /api/v3/meta/activePastes
func GetActivePastesHandler(ctx echo.Context) error {
	count, err := db.DBQueries.GetPasteCount(db.DBContext)
	if err != nil {
		logging.Logger.Errorf("Failed getting the active paste count. %d", err)
		return echo.NewHTTPError(http.StatusInternalServerError)
	}

	return ctx.JSON(http.StatusOK, activePastesResult{Count: count})
}
