namespace PasteMyst.Web.Utils;

public static class V2LanguageMapper
{
    /// <summary>
    /// Maps a v2 language name to its v3 equivalent where the names differ.
    /// Returns the mapped name, or the original if no explicit mapping exists.
    /// Callers are responsible for validating the result against <see cref="Services.LanguageProvider"/>
    /// and applying any desired fallback (e.g. "Text").
    /// </summary>
    public static string MapLanguage(string language) => language switch
    {
        "Vue.js Component" => "Vue",
        "TypeScript-JSX" => "TSX",
        "Asterisk" => "Text",
        "GitHub Flavored Markdown" => "Markdown",
        "JSON-LD" => "JSON",
        "JSONLD" => "JSON",
        "SQLite" => "SQL",
        "Properties files" => "INI",
        "Z80" => "Assembly",
        "Solr" => "Text",
        "Spreadsheet" => "Text",
        "mscgen" => "Text",
        "MS SQL" => "SQL",
        "Plain Text" => "Text",
        "Autodetect" => "Text",
        "PGP" => "Public Key",
        _ => language
    };
}
