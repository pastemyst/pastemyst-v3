package models

import (
	"time"
)

// Represents a single paste.
type Paste struct {
	Id        string    `json:"id"`
	CreatedAt time.Time `json:"createdAt"`
	ExpiresIn ExpiresIn `json:"expiresIn"`
	DeletesAt time.Time `json:"deletesAt"`
	Title     string    `json:"title"`
	Pasties   []Pasty   `json:"pasties"`
	OwnerId   string    `json:"ownerId"`
	Private   bool      `json:"private"`
}
