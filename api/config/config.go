package config

import (
	"github.com/ilyakaznacheev/cleanenv"
)

// Configuration for the application.
type Config struct {
	DbHost     string `yaml:"dbHost" env-default:"localhost"`
	DbUser     string `yaml:"dbUser" env-default:"pastemyst"`
	DbPassword string `yaml:"dbPassword" env-default:"pastemyst"`
	DbName     string `yaml:"dbName" env-default:"pastemyst"`
	DbPort     string `yaml:"dbPort" env-default:"5432"`
}

// Loads the configuration from the `config.yml` file.
func LoadConfig() (Config, error) {
	var cfg Config
	err := cleanenv.ReadConfig("config.yml", &cfg)
	return cfg, err
}
