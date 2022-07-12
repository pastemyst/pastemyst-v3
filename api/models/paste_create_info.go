package models

type PasteCreateInfo struct {
	Title   string            `json:"title"`
	Pasties []PastyCreateInfo `json:"pasties"`
}
