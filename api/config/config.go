package config

import (
	"github.com/ilyakaznacheev/cleanenv"
)

// Configuration for the application.
type Config struct {
	Host       string `yaml:"host"`
	ClientHost string `yaml:"clientHost"`

	DbHost     string `yaml:"dbHost"`
	DbUser     string `yaml:"dbUser"`
	DbPassword string `yaml:"dbPassword"`
	DbName     string `yaml:"dbName"`
	DbPort     string `yaml:"dbPort"`

	SessionSecret string `yaml:"sessionSecret"`
	JwtSecret     string `yaml:"jwtSecret"`

	GitHubClientId     string `yaml:"githubClientId"`
	GitHubClientSecret string `yaml:"githubClientSecret"`
}

var Cfg Config

// Loads the configuration from the `config.yml` file.
func LoadConfig() error {
	err := cleanenv.ReadConfig("config.yml", &Cfg)
	return err
}
