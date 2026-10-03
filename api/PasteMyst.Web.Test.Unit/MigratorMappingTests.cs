using PasteMyst.Migrator;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Models.V2;

namespace PasteMyst.Web.Test.Unit;

/// <summary>
/// Covers the pure v2 -> v3 record mappers in PasteMyst.Migrator, i.e. the field translations that
/// determine whether migrated data lands correctly in v3.
/// </summary>
public sealed class MigratorMappingTests
{
    [Test]
    public void MapUser_TranslatesFieldsAndPromotesOnlyCodeMyst()
    {
        var v2 = new UserV2
        {
            Id = "u1",
            Username = "someone",
            ServiceIds = new Dictionary<string, string> { { "github", "123" }, { "gitlab", "456" } },
            Contributor = true,
            SupporterLength = 5,
            PublicProfile = true
        };

        var user = Mappings.MapUser(v2, "avatar-id", "C#", new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc));

        Assert.Multiple(() =>
        {
            Assert.That(user.Id, Is.EqualTo("u1"));
            Assert.That(user.AvatarId, Is.EqualTo("avatar-id"));
            Assert.That(user.IsContributor, Is.True);
            Assert.That(user.IsSupporter, Is.True, "SupporterLength > 0 => supporter");
            Assert.That(user.IsAdmin, Is.False);
            // Only the first provider entry survives.
            Assert.That(user.ProviderName, Is.EqualTo("github"));
            Assert.That(user.ProviderId, Is.EqualTo("123"));
            Assert.That(user.UserSettings.ShowAllPastesOnProfile, Is.True);
            Assert.That(user.Settings.DefaultLanguage, Is.EqualTo("C#"));
            Assert.That(user.CreatedAt, Is.EqualTo(new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc)));
        });
    }

    [Test]
    public void MapUser_SupporterLengthZero_IsNotSupporter()
    {
        var v2 = new UserV2 { Id = "u", Username = "x", ServiceIds = new(), SupporterLength = 0 };
        Assert.That(Mappings.MapUser(v2, "a", "Autodetect", Mappings.V2AccountsLaunch).IsSupporter, Is.False);
    }

    [Test]
    public void MapUser_CodeMyst_IsAdmin()
    {
        var v2 = new UserV2 { Id = "u", Username = "CodeMyst", ServiceIds = new() };
        Assert.That(Mappings.MapUser(v2, "a", "Autodetect", Mappings.V2AccountsLaunch).IsAdmin, Is.True);
    }

    // Stands in for LanguageProvider: knows a few v3 names, case-insensitively, and returns the canonical name.
    private static string? Resolve(string name) =>
        new[] { "C#", "Vue", "Text", "Markdown" }.FirstOrDefault(l => l.Equals(name, StringComparison.OrdinalIgnoreCase));

    [TestCase("Autodetect", "Autodetect")]
    [TestCase(null, "Autodetect")]
    [TestCase("", "Autodetect")]
    [TestCase("C#", "C#")]
    [TestCase("c#", "C#")]                  // stored with v3's canonical casing
    [TestCase("Vue.js Component", "Vue")]   // goes through V2LanguageMapper like pasty languages
    [TestCase("Plain Text", "Text")]
    [TestCase("NotALanguage", "Autodetect")] // unknown to v3 falls back to the v3 default
    public void MapDefaultLanguage_MapsV2NameToV3(string? v2Language, string expected)
    {
        Assert.That(Mappings.MapDefaultLanguage(v2Language, Resolve), Is.EqualTo(expected));
    }

    [Test]
    public void MapUserCreatedAt_UsesOldestPaste()
    {
        Assert.That(Mappings.MapUserCreatedAt(1609459200),
            Is.EqualTo(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
    }

    [TestCase(null)]
    [TestCase(0L)]   // a few hand-made v2 pastes have createdAt = 0
    [TestCase(-1L)]
    public void MapUserCreatedAt_NoValidPaste_FallsBackToV2AccountsLaunch(long? oldestPasteCreatedAt)
    {
        Assert.That(Mappings.MapUserCreatedAt(oldestPasteCreatedAt), Is.EqualTo(Mappings.V2AccountsLaunch));
    }

    [Test]
    public void MapPasty_EmptyTitleBecomesUntitled_AndCodeBecomesContent()
    {
        var pasty = Mappings.MapPasty(new PastyV2 { Id = "p", Title = "", Language = "C#", Code = "int x;" });

        Assert.Multiple(() =>
        {
            Assert.That(pasty.Title, Is.EqualTo("untitled"));
            Assert.That(pasty.Language, Is.EqualTo("C#"));
            Assert.That(pasty.Content, Is.EqualTo("int x;"));
        });
    }

    [Test]
    public void MapPasty_NonEmptyTitleIsPreserved()
    {
        var pasty = Mappings.MapPasty(new PastyV2 { Id = "p", Title = "keep", Code = "x" });
        Assert.That(pasty.Title, Is.EqualTo("keep"));
    }

    [Test]
    public void MapUnencryptedPaste_ConvertsUnixTimes_AndNormalizesEmptyValues()
    {
        var v2 = new PasteV2
        {
            Id = "paste1",
            Title = "t",
            CreatedAt = 1_600_000_000, // fixed unix seconds
            ExpiresIn = ExpiresIn.Never,
            DeletesAt = 0,
            OwnerId = "",
            IsPrivate = true,
            IsPublic = false,
            Tags = ["x"]
        };

        var stars = new List<string> { "a", "b" };
        var paste = Mappings.MapUnencryptedPaste(v2, [], stars);

        Assert.Multiple(() =>
        {
            Assert.That(paste.CreatedAt, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(1_600_000_000).UtcDateTime));
            // deletesAt == 0 sentinel maps to null (no expiry set).
            Assert.That(paste.DeletesAt, Is.Null);
            // empty ownerId maps to null (anonymous).
            Assert.That(paste.OwnerId, Is.Null);
            Assert.That(paste.Private, Is.True);
            Assert.That(paste.Pinned, Is.False);
            Assert.That(paste.Stars, Is.EquivalentTo(stars));
        });
    }

    [Test]
    public void MapUnencryptedPaste_NonZeroDeletesAt_ConvertsToUtc_AndKeepsOwner()
    {
        var v2 = new PasteV2
        {
            Id = "p",
            Title = "t",
            CreatedAt = 1_600_000_000,
            DeletesAt = 1_600_003_600,
            OwnerId = "owner1",
            Tags = []
        };

        var paste = Mappings.MapUnencryptedPaste(v2, [], []);

        Assert.Multiple(() =>
        {
            Assert.That(paste.DeletesAt, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(1_600_003_600).UtcDateTime));
            Assert.That(paste.OwnerId, Is.EqualTo("owner1"));
        });
    }

    [Test]
    public void MapEncryptedPaste_SetsVersion2_AndMapsKeyToIv()
    {
        var v2 = new EncryptedPasteV2
        {
            Id = "enc1",
            CreatedAt = 1_600_000_000,
            DeletesAt = 0,
            OwnerId = "owner",
            Tags = [],
            EncryptedData = "data",
            EncryptedKey = "the-iv",
            Salt = "salt"
        };

        var paste = Mappings.MapEncryptedPaste(v2, []);

        Assert.Multiple(() =>
        {
            Assert.That(paste.Title, Is.EqualTo("untitled"));
            Assert.That(paste.EncryptedData, Is.EqualTo("data"));
            // v2's EncryptedKey becomes v3's Iv.
            Assert.That(paste.Iv, Is.EqualTo("the-iv"));
            Assert.That(paste.Salt, Is.EqualTo("salt"));
            // Flags the paste for lazy v2 -> v3 re-encryption on first access.
            Assert.That(paste.EncryptionVersion, Is.EqualTo(2));
        });
    }

    [Test]
    public void MapApiKey_HashesKey_SetsFullScopes_AndOwnerIsV2KeyId()
    {
        var v2 = new ApiKeyV2 { Id = "keyowner", Key = "rawkey" };

        var token = Mappings.MapApiKey(v2, "genid123");

        Assert.Multiple(() =>
        {
            Assert.That(token.Id, Is.EqualTo("genid123"));
            Assert.That(token.Description, Is.EqualTo("v2 api key"));
            Assert.That(token.ExpiresAt, Is.Null, "v2 keys never expire");
            Assert.That(token.OwnerId, Is.EqualTo("keyowner"));
            Assert.That(token.Scopes, Is.EquivalentTo(new[] { Scope.Paste, Scope.User }));
            // Token is stored as the SHA512 hex of the raw key (never the raw key itself).
            Assert.That(token.Token, Is.EqualTo(Mappings.HashToken("rawkey")));
            Assert.That(token.Token, Is.Not.EqualTo("rawkey"));
        });
    }

    [Test]
    public void HashToken_ProducesLowercase128CharHex()
    {
        var hash = Mappings.HashToken("anything");
        Assert.Multiple(() =>
        {
            Assert.That(hash, Has.Length.EqualTo(128), "SHA512 -> 64 bytes -> 128 hex chars");
            Assert.That(hash, Is.EqualTo(hash.ToLowerInvariant()));
        });
    }
}
