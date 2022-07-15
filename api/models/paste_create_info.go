package models

// Represents info used to create new pastes.
type PasteCreateInfo struct {
	Title   string            `json:"title" validate:"max=128"`
	Pasties []PastyCreateInfo `json:"pasties" validate:"required,dive,min=1"`
}
