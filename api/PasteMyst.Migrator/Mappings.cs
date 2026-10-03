using System.Security.Cryptography;
using System.Text;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Models.V2;
using PasteMyst.Web.Utils;

namespace PasteMyst.Migrator;

/// <summary>
/// Pure v2 -> v3 record mappers, extracted from the migration driver so the (risky) field
/// translations can be unit-tested without a database. I/O-derived values (downloaded avatar id,
/// resolved language, computed star list, generated token id) are passed in by the caller.
/// </summary>
public static class Mappings
{
    public static User MapUser(UserV2 v2, string avatarId, string defaultLanguage, DateTime createdAt) => new()
    {
        Id = v2.Id,
        CreatedAt = createdAt,
        Username = v2.Username,
        AvatarId = avatarId,
        IsContributor = v2.Contributor,
        IsSupporter = v2.SupporterLength > 0,
        // Only CodeMyst is promoted to admin during migration; everyone else must be set manually.
        IsAdmin = v2.Username == "CodeMyst",
        // v2 supported multiple OAuth providers; v3 keeps only the first.
        ProviderName = v2.ServiceIds.FirstOrDefault().Key,
        ProviderId = v2.ServiceIds.FirstOrDefault().Value,
        UserSettings = new UserSettings { ShowAllPastesOnProfile = v2.PublicProfile },
        Settings = new Settings { DefaultLanguage = defaultLanguage }
    };

    /// <summary>
    /// Maps a v2 default language to its v3 name. <paramref name="resolveLanguage"/> returns the v3
    /// language name for a given name, or null if v3 doesn't know it. Autodetect, missing and unknown
    /// languages all become Autodetect, the v3 default.
    /// </summary>
    public static string MapDefaultLanguage(string? v2Language, Func<string, string?> resolveLanguage)
    {
        // V2LanguageMapper turns Autodetect into Text, which is right for a pasty but not for this setting.
        if (string.IsNullOrEmpty(v2Language) || v2Language == AutodetectLanguage) return AutodetectLanguage;

        return resolveLanguage(V2LanguageMapper.MapLanguage(v2Language)) ?? AutodetectLanguage;
    }

    public const string AutodetectLanguage = "Autodetect";

    /// <summary>
    /// v2.0.0 release, which introduced accounts, so no v2 user can be older than this.
    /// </summary>
    public static readonly DateTime V2AccountsLaunch = new(2020, 11, 3, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// v2 never stored when a user joined, so use their oldest paste (unix seconds) as the join date.
    /// Users without pastes (or only with invalid timestamps) get <see cref="V2AccountsLaunch"/>.
    /// </summary>
    public static DateTime MapUserCreatedAt(long? oldestPasteCreatedAt) =>
        oldestPasteCreatedAt is > 0
            ? DateTimeOffset.FromUnixTimeSeconds(oldestPasteCreatedAt.Value).UtcDateTime
            : V2AccountsLaunch;

    public static Pasty MapPasty(PastyV2 v2) => new()
    {
        Id = v2.Id,
        Title = v2.Title == "" ? "untitled" : v2.Title,
        Language = v2.Language,
        Content = v2.Code
    };

    public static Paste MapUnencryptedPaste(PasteV2 v2, List<Pasty> pasties, List<string> stars) => new()
    {
        Id = v2.Id,
        Title = v2.Title,
        CreatedAt = DateTimeOffset.FromUnixTimeSeconds(v2.CreatedAt).UtcDateTime,
        ExpiresIn = v2.ExpiresIn,
        DeletesAt = v2.DeletesAt == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(v2.DeletesAt).UtcDateTime,
        OwnerId = v2.OwnerId == "" ? null : v2.OwnerId,
        Private = v2.IsPrivate,
        Pinned = v2.IsPublic,
        Tags = v2.Tags,
        Stars = stars,
        Pasties = pasties
    };

    public static EncryptedPaste MapEncryptedPaste(EncryptedPasteV2 v2, List<string> stars) => new()
    {
        Id = v2.Id,
        Title = "untitled",
        CreatedAt = DateTimeOffset.FromUnixTimeSeconds(v2.CreatedAt).UtcDateTime,
        ExpiresIn = v2.ExpiresIn,
        DeletesAt = v2.DeletesAt == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(v2.DeletesAt).UtcDateTime,
        OwnerId = v2.OwnerId == "" ? null : v2.OwnerId,
        Private = v2.IsPrivate,
        Pinned = v2.IsPublic,
        Tags = v2.Tags,
        Stars = stars,
        EncryptedData = v2.EncryptedData,
        Iv = v2.EncryptedKey,
        Salt = v2.Salt,
        // Marks the paste as v2-encrypted so v3 lazily re-encrypts it to version 3 on first access.
        EncryptionVersion = 2
    };

    public static AccessToken MapApiKey(ApiKeyV2 v2, string id) => new()
    {
        Id = id,
        Description = "v2 api key",
        Hidden = false,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = null,
        Token = HashToken(v2.Key),
        // v2 keys carry no scopes, so they get full [Paste, User] access for compatibility.
        OwnerId = v2.Id,
        Scopes = [Scope.Paste, Scope.User]
    };

    /// <summary>
    /// SHA512 -> lowercase hex, matching AuthService's token hashing so migrated keys validate.
    /// </summary>
    public static string HashToken(string token)
    {
        var hashed = SHA512.HashData(Encoding.UTF8.GetBytes(token));
        var sb = new StringBuilder();
        foreach (var b in hashed) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
