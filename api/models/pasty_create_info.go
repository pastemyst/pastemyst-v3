package models

type PastyCreateInfo struct {
	Title   string `json:"title" validate:"max=50"`
	Content string `json:"content"`
}
