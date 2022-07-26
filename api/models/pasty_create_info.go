package models

// Represents info used to create a pasty.
type PastyCreateInfo struct {
	Title    string `json:"title" validate:"max=50"`
	Content  string `json:"content"`
	Language string `json:"language"`
}
