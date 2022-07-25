package models

// Represents a single language.
type Language struct {
	Name               string   `json:"name"`
	Type               string   `yaml:"type" json:"type"`
	Aliases            []string `yaml:"aliases" json:"aliases"`
	CodemirrorMode     string   `yaml:"codemirror_mode" json:"codemirrorMode"`
	CodemirrorMimeType string   `yaml:"codemirror_mime_type" json:"codemirrorMimeType"`
	Wrap               bool     `yaml:"wrap" json:"wrap"`
	Extensions         []string `yaml:"extensions" json:"extensions"`
	Color              string   `yaml:"color" json:"color"`
	TmScope            string   `yaml:"tm_scope" json:"tmScope"`
}
