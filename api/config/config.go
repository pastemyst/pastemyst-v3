package config

import (
	"github.com/ilyakaznacheev/cleanenv"
)

// Configuration for the application.
type Config struct {
	Api struct {
		Host  string `toml:"host"`
		Port  uint64 `toml:"port"`
		Https bool   `toml:"https"`
	} `toml:"api"`

	Client struct {
		Host string `toml:"host"`
	} `toml:"client"`

	Database struct {
		Host     string `toml:"host"`
		Port     uint64 `toml:"port"`
		User     string `toml:"user"`
		Password string `toml:"password"`
		Database string `toml:"database"`
	} `toml:"database"`

	Secrets struct {
		Jwt     string `toml:"jwt"`
		Session string `toml:"session"`
	} `toml:"secrets"`

	Github struct {
		Id     string `toml:"id"`
		Secret string `toml:"secret"`
	} `toml:"github"`

	Gitlab struct {
		Id     string `toml:"id"`
		Secret string `toml:"secret"`
	} `toml:"gitlab"`
}

var Cfg Config

// Loads the configuration from the `config.toml` file.
func LoadConfig() error {
	err := cleanenv.ReadConfig("config.toml", &Cfg)
	return err
}
