package models

// When the paste should expire.
type ExpiresIn string

const (
	ExpiresInNever ExpiresIn = "never"
	ExpiresIn1h    ExpiresIn = "1h"
	ExpiresIn2h    ExpiresIn = "2h"
	ExpiresIn10h   ExpiresIn = "10h"
	ExpiresIn1d    ExpiresIn = "1d"
	ExpiresIn2d    ExpiresIn = "2d"
	ExpiresIn1w    ExpiresIn = "1w"
	ExpiresIn1m    ExpiresIn = "1m"
	ExpiresIn1y    ExpiresIn = "1y"
)
