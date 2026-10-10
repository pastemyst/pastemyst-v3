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

    /// <summary>
    /// Maps a v3 language name back to its v2 name, for v2 API responses. Only for v3 names that
    /// didn't exist in v2, where v2 clients (e.g. ones parsing names into an enum) would choke.
    /// Assembly stays Assembly: its only v2 source was Z80, which would be wrong for other pastes.
    /// </summary>
    public static string ToV2Name(string language) => language switch
    {
        "Text" => "Plain Text",
        "Vue" => "Vue.js Component",
        "TSX" => "TypeScript-JSX",
        "INI" => "Properties files",
        "Public Key" => "PGP",
        _ => language
    };
}
