package validation

import (
	"mime/multipart"
	"net/http"
	"strings"
)

// Checks if the provided file is a valid image file.
func IsValidImage(file multipart.File) bool {
	buff := make([]byte, 512)
	if _, err := file.Read(buff); err != nil {
		return false
	}

	mime := http.DetectContentType(buff)

	file.Seek(0, 0)

	return strings.Split(mime, "/")[0] == "image"
}
