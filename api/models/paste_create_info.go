package models

// Represents info used to create new pastes.
type PasteCreateInfo struct {
	Title     string            `json:"title" validate:"max=128"`
	Pasties   []PastyCreateInfo `json:"pasties" validate:"required,dive,min=1"`
	ExpiresIn ExpiresIn         `json:"expiresIn" validate:"omitempty,oneof=never 1h 2h 10h 1d 2d 1w 1m 1y"`
}
