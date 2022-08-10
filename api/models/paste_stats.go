package models

// Various statistics of a single paste.
type PasteStats struct {
	Stats
	Pasties map[string]Stats `json:"pasties"`
}

type Stats struct {
	Lines uint64 `json:"lines"`
	Words uint64 `json:"words"`
	Size  uint64 `json:"size"`
}
