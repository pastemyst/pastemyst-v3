package auth

import (
	"net/http"
	"pastemyst-api/config"
	"pastemyst-api/db"
	"pastemyst-api/logging"
	"pastemyst-api/models"
	"strings"
	"time"

	"github.com/golang-jwt/jwt/v4"
	"github.com/labstack/echo/v4"
)

// Gets the user from the JWT token provided in the cookie or Authorization header, and sets the user in the context.
func AuthMiddleware(next echo.HandlerFunc) echo.HandlerFunc {
	return func(ctx echo.Context) error {
		var tokenString string

		// first try the cookie, if it doesn't exist then try the auth header
		cookie, err := ctx.Cookie("pastemyst")
		if err != nil {
			authHeader := ctx.Request().Header.Get("Authorization")
			if authHeader == "" {
				ctx.Set("user", nil)
				return next(ctx)
			}

			if !strings.HasPrefix(authHeader, "Bearer ") {
				return echo.NewHTTPError(http.StatusBadRequest, "Wrong Authorization scheme. The token must be provided as a Bearer token.")
			}

			tokenString = authHeader[len("Bearer "):]
		} else {
			tokenString = cookie.Value
		}

		// decode jwt
		claims := &Claims{}
		token, err := jwt.ParseWithClaims(tokenString, claims, func(t *jwt.Token) (interface{}, error) {
			return []byte(config.Cfg.JwtSecret), nil
		})
		if err != nil || !token.Valid {
			logging.Logger.Errorf("User tried to authorize with an invalid JWT token: %s", err.Error())
			return echo.NewHTTPError(http.StatusUnauthorized, "You tried to authorize with an invalid JWT token.")
		}

		// get the user and save it into the context
		dbUser, err := db.DBQueries.GetUserById(db.DBContext, claims.Id)
		if err != nil {
			// valid jwt, but the user doesn't exist anymore, delete the cookie
			logging.Logger.Error("Got a valid JWT token, but the user doesn't exist anymore. Deleting the cookie and resuming the auth process.")

			cookie := &http.Cookie{}
			cookie.Name = "pastemyst"
			cookie.Path = "/"
			cookie.HttpOnly = true
			cookie.SameSite = http.SameSiteStrictMode
			cookie.Secure = config.Cfg.Https
			cookie.Value = ""
			cookie.Expires = time.Unix(0, 0)

			ctx.SetCookie(cookie)

			return next(ctx)
		}

		user := models.User{
			Id:        dbUser.ID,
			CreatedAt: dbUser.CreatedAt,
			Username:  dbUser.Username,
			AvatarUrl: dbUser.AvatarUrl,
		}

		ctx.Set("user", user)

		return next(ctx)
	}
}
