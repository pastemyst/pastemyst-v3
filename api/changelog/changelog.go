package changelog

import (
	"encoding/json"
	"fmt"
	"net/http"
	"os/exec"
	"pastemyst-api/logging"
	"strings"
	"time"

	"github.com/labstack/echo/v4"
)

// Information about a single app release.
type Release struct {
	URL          string    `json:"url"`           // Link to the GitHub release.
	Title        string    `json:"title"`         // Release Title.
	Content      string    `json:"content"`       // Markdown content.
	IsPrerelease bool      `json:"is_prerelease"` // Whether it a pre-release (alpha).
	ReleasedAt   time.Time `json:"released_at"`   // When the release was published.
}

// Data returned from GitHub when fetching all the releases.
type githubRelease struct {
	URL         string    `json:"html_url"`
	Title       string    `json:"name"`
	Tag         string    `json:"tag_name"`
	Body        string    `json:"body"`
	Draft       bool      `json:"draft"`
	Prerelease  bool      `json:"prerelease"`
	PublishedAt time.Time `json:"published_at"`
}

// List of all app releases.
var Releases []Release

// Current app version.
var Version string

// Initializes the current version and changelog.
func InitChangelog() error {
	ver, err := getVersion()
	if err != nil {
		return err
	}
	Version = ver

	rels, err := generateChangelog()
	if err != nil {
		return err
	}
	Releases = rels

	return nil
}

// Gets the current version from git tags.
func getVersion() (string, error) {
	cmd := exec.Command("git", "describe", "--tags", "--abbrev=0")
	stdout, err := cmd.CombinedOutput()
	if err != nil {
		return "", err
	}

	return fmt.Sprintf("v%s", strings.TrimSpace(string(stdout))), nil
}

func generateChangelog() ([]Release, error) {
	res, err := http.Get("https://api.github.com/repos/pastemyst/pastemyst-v3/releases")
	if err != nil {
		return nil, err
	}

	defer res.Body.Close()

	if res.StatusCode != http.StatusOK {
		logging.Logger.Errorf("Failed getting the list of all GitHub releases. Got status: %s", res.Status)
		return nil, echo.NewHTTPError(res.StatusCode)
	}

	ghReleases := []githubRelease{}
	json.NewDecoder(res.Body).Decode(&ghReleases)

	releases := make([]Release, 0, len(ghReleases))
	for _, ghRel := range ghReleases {
		// ignore drafts
		if ghRel.Draft {
			continue
		}

		// if it doesn't have a title use the tag as the title
		title := ghRel.Title
		if title == "" {
			title = ghRel.Tag
		}

		// prepend with v if needed
		if title[0] != 'v' {
			title = "v" + title
		}

		releases = append(releases, Release{
			URL:          ghRel.URL,
			Title:        title,
			Content:      ghRel.Body,
			IsPrerelease: ghRel.Prerelease,
			ReleasedAt:   ghRel.PublishedAt,
		})
	}

	return releases, nil
}
