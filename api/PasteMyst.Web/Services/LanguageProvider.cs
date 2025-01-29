using System.Diagnostics;
using PasteMyst.Web.Extensions;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PasteMyst.Web.Services;

public class LanguageProvider : IHostedService
{
    private const string LanguagesUri =
        "https://raw.githubusercontent.com/github-linguist/linguist/v7.29.0/lib/linguist/languages.yml";

    public List<Language> Languages { get; private set; } = new();

    public List<string> PopularLanguageNames =>
    [
        "Autodetect", "Text", "C", "C#", "C++", "CSS", "D", "Dart", "Go", "Haskell", "HTML", "Java",
        "JavaScript", "JSON", "Kotlin", "Markdown", "Objective-C", "Perl", "PHP", "PowerShell",
        "Python", "Ruby", "Rust", "Scala", "Shell", "Swift", "TypeScript", "Yaml"
    ];

    /// <summary>
    /// Tries to find a language based on the name (it will search names, aliases and extensions).
    /// </summary>
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

    public async Task<Language> AutodetectLanguageAsync(string content, CancellationToken token)
    {
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, content, token);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "guesslang-bun",
                Arguments = tempFile,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        var languageName = (await process.StandardOutput.ReadToEndAsync(token)).Trim();

        return FindByName(languageName);
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

        Languages.Insert(0, new Language
        {
            Name = "Autodetect",
            Aliases = new List<string> { "autodetect" },
            Extensions = new List<string> {}
        });
    }
}
