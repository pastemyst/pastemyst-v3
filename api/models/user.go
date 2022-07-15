package models

import "time"

type User struct {
	Id        string    `json:"id"`
	CreatedAt time.Time `json:"createdAt"`
	Username  string    `json:"username"`
	AvatarUrl string    `json:"avatarUrl"`
}
