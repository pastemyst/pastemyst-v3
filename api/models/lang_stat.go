package models

// Holds language statistics for pastes.
type LangStat struct {
	Language   Language `json:"language"`
	Percentage float32  `json:"percentage"`
}
