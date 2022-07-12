package validation

import (
	"fmt"
	"net/http"
	"strings"

	"github.com/go-playground/validator/v10"
	"github.com/labstack/echo/v4"
)

type CustomValidator struct {
	Validator *validator.Validate
}

func (cv *CustomValidator) Validate(i interface{}) error {
	if err := cv.Validator.Struct(i); err != nil {
		var sb strings.Builder

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
			}

			sb.WriteString(fmt.Sprintf("%s: %s.", err.Namespace(), msg))
		}

		return echo.NewHTTPError(http.StatusBadRequest, sb.String())
	}

	return nil
}
