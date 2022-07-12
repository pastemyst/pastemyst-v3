package handlers

import "github.com/labstack/echo/v4"

type Message struct {
	Message string `json:"message"`
}

func MessageResponse(ctx echo.Context, status int, message string) error {
	return ctx.JSON(status, Message{Message: message})
}
