package changelog

import (
	"fmt"
	"os/exec"
	"strings"
	"time"
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

func InitChangelog() error {
	ver, err := getVersion()
	if err != nil {
		return err
	}
	Version = ver

	return nil
}

func getVersion() (string, error) {
	cmd := exec.Command("git", "describe", "--tags", "--abbrev=0")
	stdout, err := cmd.CombinedOutput()
	if err != nil {
		return "", err
	}

	return fmt.Sprintf("v%s", strings.TrimSpace(string(stdout))), nil
}
