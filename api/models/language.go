package models

// Represents a single language.
type Language struct {
	Name               string
	Type               string   `yaml:"type"`
	Aliases            []string `yaml:"aliases"`
	CodemirrorMode     string   `yaml:"codemirror_mode"`
	CodemirrorMimeType string   `yaml:"codemirror_mime_type"`
	Wrap               bool     `yaml:"wrap"`
	Extensions         []string `yaml:"extensions"`
	Color              string   `yaml:"color"`
	TmScope            string   `yaml:"tm_scope"`
}
