package auth

import (
	"fmt"
	"pastemyst-api/config"

	"golang.org/x/oauth2"
	"golang.org/x/oauth2/github"
)

var GithubOauthConfig *oauth2.Config

func InitAuth() {
	GithubOauthConfig = &oauth2.Config{
		ClientID:     config.Cfg.GitHubClientId,
		ClientSecret: config.Cfg.GitHubClientSecret,
		Endpoint:     github.Endpoint,
		RedirectURL:  fmt.Sprintf("%s/api/v3/login/github-callback", config.Cfg.Host),
		Scopes:       []string{"read:user"},
	}
}
