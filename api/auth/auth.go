package auth

import (
	"encoding/json"
	"fmt"
	"net/http"
	"pastemyst-api/config"
	"pastemyst-api/logging"

	"github.com/golang-jwt/jwt/v4"
	"github.com/labstack/echo"
	"golang.org/x/oauth2"
	"golang.org/x/oauth2/github"
	"golang.org/x/oauth2/gitlab"
	"golang.org/x/oauth2/google"
)

// User representation of a OAuth provider user.
type ProviderUser struct {
	Id        string
	Username  string
	AvatarUrl string
}

// Configuration for OAuth providers.
// Adds more fields to oauth2.Config struct.
type ProviderConfig struct {
	oauth2.Config

	Name               string
	UserUrl            string
	IdJsonField        string
	UsernameJsonField  string
	AvatarUrlJsonField string
}

// JWT Claims for auth.
type Claims struct {
	jwt.RegisteredClaims

	Id       string `json:"id"`
	Username string `json:"username"`
}

// JWT Claims used for storing info when creating a new user.
type RegistrationClaims struct {
	jwt.RegisteredClaims

	ProviderName string `json:"provider_name"`
	ProviderId   string `json:"provider_id"`
	AvatarUrl    string `json:"avatar_url"`
}

// Config for GitHub OAuth
var GithubOauthConfig *ProviderConfig

// Config for GitLab OAuth
var GitlabOauthConfig *ProviderConfig

// Config for Google OAuth
var GoogleOauthConfig *ProviderConfig

// List of all configurations of OAuth proivders as a map
// for easy conversion of provider name to provider config.
var OAuthProviders map[string]*ProviderConfig

// Inits OAuth configs.
func InitAuth() {
	GithubOauthConfig = &ProviderConfig{
		Config: oauth2.Config{
			ClientID:     config.Cfg.GitHubClientId,
			ClientSecret: config.Cfg.GitHubClientSecret,
			Endpoint:     github.Endpoint,
			RedirectURL:  fmt.Sprintf("%s/api/v3/login/github/callback", config.Cfg.Host),
			Scopes:       []string{"read:user"},
		},
		Name:               "GitHub",
		UserUrl:            "https://api.github.com/user",
		IdJsonField:        "id",
		UsernameJsonField:  "login",
		AvatarUrlJsonField: "avatar_url",
	}

	GitlabOauthConfig = &ProviderConfig{
		Config: oauth2.Config{
			ClientID:     config.Cfg.GitLabClientId,
			ClientSecret: config.Cfg.GitLabClientSecret,
			Endpoint:     gitlab.Endpoint,
			RedirectURL:  fmt.Sprintf("%s/api/v3/login/gitlab/callback", config.Cfg.Host),
			Scopes:       []string{"read_user"},
		},
		Name:               "GitLab",
		UserUrl:            "https://gitlab.com/api/v4/user",
		IdJsonField:        "id",
		UsernameJsonField:  "username",
		AvatarUrlJsonField: "avatar_url",
	}

	GoogleOauthConfig = &ProviderConfig{
		Config: oauth2.Config{
			ClientID:     config.Cfg.GoogleClientId,
			ClientSecret: config.Cfg.GoogleClientSecret,
			Endpoint:     google.Endpoint,
			RedirectURL:  fmt.Sprintf("%s/api/v3/login/google/callback", config.Cfg.Host),
			Scopes:       []string{"https://www.googleapis.com/auth/userinfo.profile"},
		},
		Name:               "Google",
		UserUrl:            "https://www.googleapis.com/oauth2/v1/userinfo?alt=json",
		IdJsonField:        "id",
		UsernameJsonField:  "name",
		AvatarUrlJsonField: "picture",
	}

	OAuthProviders = map[string]*ProviderConfig{
		"github": GithubOauthConfig,
		"gitlab": GitlabOauthConfig,
		"google": GoogleOauthConfig,
	}
}

// Returns the OAuth2 provider user.
func GetProviderUser(providerCfg *ProviderConfig, token string) (*ProviderUser, error) {
	httpClient := &http.Client{}

	// make a request to the provider endpoint for the user
	req, err := http.NewRequest(http.MethodGet, providerCfg.UserUrl, nil)
	if err != nil {
		logging.Logger.Errorf("Failed creating http.NewRequest: %s", err.Error())
		return nil, echo.NewHTTPError(http.StatusInternalServerError)
	}

	// set proper headers
	if providerCfg == GithubOauthConfig {
		req.Header.Set("Accept", "application/vnd.github.v3+json")
		req.Header.Set("Authorization", fmt.Sprintf("token %s", token))
	} else {
		req.Header.Set("Accept", "application/json")
		req.Header.Set("Authorization", fmt.Sprintf("Bearer %s", token))
	}

	res, err := httpClient.Do(req)
	if err != nil {
		logging.Logger.Errorf("Failed getting the OAuth2 user: %s", err.Error())
		return nil, echo.NewHTTPError(http.StatusInternalServerError)
	}

	defer res.Body.Close()

	// convert to json
	var jsonUser map[string]interface{}
	json.NewDecoder(res.Body).Decode(&jsonUser)

	user := &ProviderUser{
		Username:  jsonUser[providerCfg.UsernameJsonField].(string),
		AvatarUrl: jsonUser[providerCfg.AvatarUrlJsonField].(string),
	}

	// github's and gitlab's IDs are numbers (which for some reason have to be parsed like floats)
	if providerCfg == GithubOauthConfig || providerCfg == GitlabOauthConfig {
		id := jsonUser[providerCfg.IdJsonField].(float64)
		user.Id = fmt.Sprint(int(id))
	} else {
		user.Id = jsonUser[providerCfg.IdJsonField].(string)
	}

	return user, nil
}
