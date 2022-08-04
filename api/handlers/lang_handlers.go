package handlers

import (
	"net/http"
	"pastemyst-api/language"
	"pastemyst-api/logging"
	"pastemyst-api/models"
	"sort"

	"github.com/labstack/echo/v4"
)

// Returns the list of all languages.
//
// /api/v3/lang/all
func GetAllLangsHandler(ctx echo.Context) error {
	// let browsers know this resource can be cached for a week max
	ctx.Response().Header().Add("Cache-Control", "max-age=604800")

	return ctx.JSON(http.StatusOK, language.Languages)
}

// Returns language statistics of the provided paste.
//
// /api/v3/lang/:id
func GetLangStatsHandler(ctx echo.Context) error {
	id := ctx.Param("id")

	user, ok := ctx.Get("user").(models.User)

	var paste models.Paste
	var err error

	if ok {
		paste, err = GetPaste(id, &user)
	} else {
		paste, err = GetPaste(id, nil)
	}

	if err != nil {
		return err
	}

	var stats []models.LangStat

	// go through each pasty and count it's length, and add the total for each language
	charCount := make(map[string]int)
	totalChars := 0

	for _, pasty := range paste.Pasties {
		_, exists := charCount[pasty.Language]

		if !exists {
			charCount[pasty.Language] = 0
		}

		charCount[pasty.Language] += len(pasty.Content)
		totalChars += len(pasty.Content)
	}

	if totalChars == 0 {
		return ctx.JSON(http.StatusOK, []models.LangStat{})
	}

	for lang, count := range charCount {
		perc := (float32(count) / float32(totalChars)) * 100

		if perc != 0 {
			fullLang, err := language.FindLanguage(lang)
			if err != nil {
				logging.Logger.Errorf("Failed to find language when calculating stats: %s", err)
				return echo.NewHTTPError(http.StatusInternalServerError, "Failed finding the language.")
			}

			stats = append(stats, models.LangStat{
				Language:   fullLang,
				Percentage: perc,
			})
		}
	}

	sort.Slice(stats, func(i, j int) bool {
		return stats[i].Percentage > stats[j].Percentage
	})

	return ctx.JSON(http.StatusOK, stats)
}
