package config

import (
	"github.com/ilyakaznacheev/cleanenv"
)

// Configuration for the application.
type Config struct {
	Host       string `yaml:"host"`
	Port       uint16 `yaml:"port"`
	ClientHost string `yaml:"clientHost"`
	Https      bool   `yaml:"https"`

	DbHost     string `yaml:"dbHost"`
	DbUser     string `yaml:"dbUser"`
	DbPassword string `yaml:"dbPassword"`
	DbName     string `yaml:"dbName"`
	DbPort     string `yaml:"dbPort"`

	SessionSecret string `yaml:"sessionSecret"`
	JwtSecret     string `yaml:"jwtSecret"`

	GitHubClientId     string `yaml:"githubClientId"`
	GitHubClientSecret string `yaml:"githubClientSecret"`

	GitLabClientId     string `yaml:"gitlabClientId"`
	GitLabClientSecret string `yaml:"gitlabClientSecret"`

	GoogleClientId     string `yaml:"googleClientId"`
	GoogleClientSecret string `yaml:"googleClientSecret"`
}

var Cfg Config

// Loads the configuration from the `config.yml` file.
func LoadConfig() error {
	err := cleanenv.ReadConfig("config.yml", &Cfg)
	return err
}
