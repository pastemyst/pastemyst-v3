using pastemyst.Exceptions;
using pastemyst.Extensions;
using pastemyst.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace pastemyst.Services;

public interface ILanguageProvider
{
    public List<Language> Languages { get; }

    public List<string> PopularLanguageNames { get; }

    /// <summary>
    /// Tries to find a language based on the name (it will search names, aliases and extensions).
    /// </summary>
    public Language FindByName(string name);
}

public class LanguageProvider : ILanguageProvider, IHostedService
{
    private const string LanguagesUri =
        "https://raw.githubusercontent.com/github-linguist/linguist/v7.29.0/lib/linguist/languages.yml";

    public List<Language> Languages { get; private set; } = new();

    public List<string> PopularLanguageNames =>
    [
        "Text", "C", "C#", "C++", "CSS", "D", "Dart", "Go", "Haskell", "HTML", "Java",
        "JavaScript", "JSON", "Kotlin", "Markdown", "Objective-C", "Perl", "PHP", "PowerShell",
        "Python", "Ruby", "Rust", "Scala", "Shell", "Swift", "TypeScript", "Yaml"
    ];

    public Language FindByName(string name)
    {
        Language foundLang = null;

        foreach (var language in Languages)
        {
            // If found a match based on the name, return it since it's the best match
            if (name.EqualsIgnoreCase(language.Name))
            {
                return language;
            }

            if (foundLang is not null) continue;

            // Check of aliases and extensions.
            // If found, keep searching, maybe in the next iterations there will be a better match.

            // Ignore the dot from the extension.
            if (language.Extensions is not null &&
                language.Extensions.Any(extension => name.EqualsIgnoreCase(extension[1..])))
            {
                foundLang = language;
            }
            
            if (language.Aliases is not null &&
                language.Aliases.Any(name.EqualsIgnoreCase))
            {
                foundLang = language;
            }
        }

        if (foundLang is null) throw new LanguageNotFoundException();

        return foundLang;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await LoadLanguages();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task LoadLanguages()
    {
        var languagesYaml = await new HttpClient().GetStringAsync(LanguagesUri);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var res = deserializer.Deserialize<Dictionary<string, Language>>(languagesYaml);

        foreach (var pair in res)
        {
            pair.Value.Name = pair.Key;
        }

        Languages = res.Values.ToList();
        Languages.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
    }
}
