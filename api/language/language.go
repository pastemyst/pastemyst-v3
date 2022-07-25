package language

import (
	"io/ioutil"
	"net/http"
	"os"
	"pastemyst-api/models"
	"sort"

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
