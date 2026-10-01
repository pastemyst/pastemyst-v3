using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Services;
using PasteMyst.Web.Utils;

namespace PasteMyst.Web.Test.Unit;

public sealed class V2LanguageMapperTests
{
    private LanguageProvider _languageProvider;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _languageProvider = new LanguageProvider();
        Task.Run(() => _languageProvider.StartAsync(CancellationToken.None)).Wait();
    }

    // Every explicit mapping in V2LanguageMapper. If a mapping is added/changed/removed,
    // this list must be updated to match, which is the point: the mapping is locked down.
    private static readonly object[] KnownMappings =
    [
        new object[] { "Vue.js Component", "Vue" },
        new object[] { "TypeScript-JSX", "TSX" },
        new object[] { "Asterisk", "Text" },
        new object[] { "GitHub Flavored Markdown", "Markdown" },
        new object[] { "JSON-LD", "JSON" },
        new object[] { "JSONLD", "JSON" },
        new object[] { "SQLite", "SQL" },
        new object[] { "Properties files", "INI" },
        new object[] { "Z80", "Assembly" },
        new object[] { "Solr", "Text" },
        new object[] { "Spreadsheet", "Text" },
        new object[] { "mscgen", "Text" },
        new object[] { "MS SQL", "SQL" },
        new object[] { "Plain Text", "Text" },
        new object[] { "Autodetect", "Text" },
        new object[] { "PGP", "Public Key" }
    ];

    [TestCaseSource(nameof(KnownMappings))]
    public void MapLanguage_MapsKnownV2Name_ToV3Equivalent(string v2Name, string expectedV3Name)
    {
        Assert.That(V2LanguageMapper.MapLanguage(v2Name), Is.EqualTo(expectedV3Name));
    }

    [TestCase("JavaScript")]
    [TestCase("C#")]
    [TestCase("Rust")]
    [TestCase("Text")]
    [TestCase("Markdown")]
    public void MapLanguage_LeavesUnmappedName_Unchanged(string name)
    {
        // Names that already match v3 (or are simply unknown) must pass through untouched.
        Assert.That(V2LanguageMapper.MapLanguage(name), Is.EqualTo(name));
    }

    [Test]
    public void MapLanguage_EveryMappedOutput_ResolvesInLanguageProvider()
    {
        // Guard: a mapping must never point at a language name v3 doesn't know about,
        // otherwise migrated pastes would carry a language the app can't render.
        var outputs = KnownMappings
            .Cast<object[]>()
            .Select(pair => (string)pair[1])
            .Distinct();

        Assert.Multiple(() =>
        {
            foreach (var output in outputs)
            {
                Assert.DoesNotThrow(
                    () => _languageProvider.FindByName(output),
                    $"Mapped language \"{output}\" is not recognised by LanguageProvider.");
            }
        });
    }
}
