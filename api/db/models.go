// Code generated by sqlc. DO NOT EDIT.
// versions:
//   sqlc v1.14.0

package db

import (
	"time"
)

type Paste struct {
	ID        string
	CreatedAt time.Time
	Title     string
}

type Pasty struct {
	ID      string
	PasteID string
	Title   string
	Content string
}
