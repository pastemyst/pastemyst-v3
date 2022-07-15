package validation

import (
	"fmt"
	"net/http"
	"strings"

	"github.com/go-playground/validator/v10"
	"github.com/labstack/echo/v4"
)

// Custom validator for echo.
type CustomValidator struct {
	Validator *validator.Validate
}

// Runs custom validation of structs.
func (cv *CustomValidator) Validate(i interface{}) error {
	if err := cv.Validator.Struct(i); err != nil {
		return echo.NewHTTPError(http.StatusBadRequest, ValidationErrToMsg(err))
	}

	return nil
}

func ValidationErrToMsg(err error) string {
	var sb strings.Builder

	// If there were errors, go through each of them and convert it to a string.
	for i, err := range err.(validator.ValidationErrors) {
		if i != 0 {
			sb.WriteString(" ")
		}

		var msg string
		switch err.Tag() {
		case "required":
			msg = "required"
		case "min":
			msg = fmt.Sprintf("minimum length %s", err.Param())
		case "max":
			msg = fmt.Sprintf("maximum length %s", err.Param())
		case "alphanum":
			msg = "must contain only alphanumeric characters"
		case "containsany":
			msg = fmt.Sprintf("must contain only these characters: %s", err.Param())
		}

		sb.WriteString(fmt.Sprintf("%s: %s.", err.Namespace(), msg))
	}

	return sb.String()
}
