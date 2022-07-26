package language

import (
	"errors"
	"io/ioutil"
	"net/http"
	"os"
	"pastemyst-api/models"
	"sort"
	"strings"

	"gopkg.in/yaml.v3"
)

var Languages []models.Language

const languagesPath = "languages.yml"
const languagesGithubRaw = "https://raw.githubusercontent.com/github/linguist/master/lib/linguist/languages.yml"

// Loads all languages to be used from the Linguist languages.yml file.
// The file is automatically downloaded each start of the app.
func LoadLanguages() error {
	// download the latest languages.yml from the linguist github page
	resp, err := http.Get(languagesGithubRaw)
	if err != nil {
		return err
	}
	defer resp.Body.Close()

	content, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return err
	}

	err = os.WriteFile(languagesPath, content, 0644)
	if err != nil {
		return err
	}

	// convert yaml to map
	// the yaml file is in the form of:
	//    name:
	//      params...
	data := make(map[string]models.Language)
	err = yaml.Unmarshal(content, &data)
	if err != nil {
		return err
	}

	// convert the map[string]Language to []Language
	Languages = make([]models.Language, len(data))

	i := 0
	for name, lang := range data {
		lang.Name = name
		Languages[i] = lang

		i++
	}

	sort.Slice(Languages, func(i, j int) bool {
		return Languages[i].Name < Languages[j].Name
	})

	return nil
}

// Tries to find a language based on the name, it will search names, aliases and extensions.
func FindLanguage(langName string) (models.Language, error) {
	var foundLang models.Language
	var found = false

	for _, lang := range Languages {
		// if name matches, return the lang
		if strings.EqualFold(lang.Name, langName) {
			return lang, nil
		}

		// check for aliases, if found, keep looping
		// maybe in a next iteration we will find a better match by name
		if !found && len(lang.Aliases) > 0 {
			for _, alias := range lang.Aliases {
				if strings.EqualFold(alias, langName) {
					found = true
					foundLang = lang
					break
				}
			}
		}

		// check for extensions, if found, keep looping
		// maybe in a next iteration we will find a better match by name
		if !found && len(lang.Extensions) > 0 {
			for _, ext := range lang.Extensions {
				// ignore first dot in extension
				if strings.EqualFold(ext[1:], langName) {
					found = true
					foundLang = lang
					break
				}
			}
		}
	}

	if found {
		return foundLang, nil
	} else {
		return models.Language{}, errors.New("couldn't find the language")
	}
}
