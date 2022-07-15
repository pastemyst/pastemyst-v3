package models

// Represents a single pasty (file inside a paste).
type Pasty struct {
	Id      string `json:"id"`
	Title   string `json:"title"`
	Content string `json:"content"`
}
