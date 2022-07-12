package handlers

import (
	"net/http"
	"pastemyst-api/changelog"

	"github.com/labstack/echo/v4"
)

type versionResult struct {
	Version string `json:"version"`
}

type releasesResult struct {
	Releases []changelog.Release `json:"releases"`
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
