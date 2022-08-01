package models

import "time"

type User struct {
	Id          string    `json:"id"`
	CreatedAt   time.Time `json:"createdAt"`
	Username    string    `json:"username"`
	AvatarUrl   string    `json:"avatarUrl"`
	Contributor bool      `json:"contributor"`
	Supporter   uint32    `json:"supporter"`
}
