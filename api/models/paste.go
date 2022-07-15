package models

import "time"

// Represents a single paste.
type Paste struct {
	Id        string    `json:"id"`
	CreatedAt time.Time `json:"createdAt"`
	Title     string    `json:"title"`
	Pasties   []Pasty   `json:"pasties"`
}
