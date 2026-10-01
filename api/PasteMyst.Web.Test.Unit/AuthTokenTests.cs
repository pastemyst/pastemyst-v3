using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Test.Unit;

/// <summary>
/// Covers <see cref="AuthService"/> access-token validation, the compat path that lets both
/// v3 tokens ({8-char-id}-{secret}) and migrated v2 tokens (bare string, SHA512-hashed) authenticate.
/// Exercised through the public <see cref="AuthService.GetSelfWithScopesAsync"/> so the real
/// private validation logic runs.
/// </summary>
public sealed class AuthTokenTests
{
    private readonly DatabaseFixture _databaseFixture = new();
    private AuthService _authService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // GetSelfWithScopesAsync only touches MongoService on the validation path, so the other
        // dependencies are not needed for these tests.
        _authService = new AuthService(
            idProvider: null,
            oAuthService: null,
            configuration: null,
            httpClientFactory: null,
            imageService: null,
            actionLogger: null,
            userContext: new UserContext(),
            mongo: _databaseFixture.MongoService);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _databaseFixture.Dispose();
    }

    // Same SHA512 -> lowercase hex encoding used by both AuthService and the migrator.
    private static string Sha512Hex(string input)
    {
        var hash = SHA512.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder();
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    private static HttpContext ContextWithAuthHeader(string headerValue)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Authorization = headerValue;
        return ctx;
    }

    private async Task<User> SeedUser(string id)
    {
        var user = new User { Id = id, Username = id };
        await _databaseFixture.MongoService.Users.InsertOneAsync(user);
        return user;
    }

    [Test]
    public async Task GetSelf_AcceptsV3Token_AndReturnsOwnerWithScopes()
    {
        await SeedUser("v3owner");

        const string secret = "supersecretrawtokenvalue";
        await _databaseFixture.MongoService.AccessTokens.InsertOneAsync(new AccessToken
        {
            Id = "v3tok123", // 8 chars -> triggers the v3 branch
            Token = Sha512Hex(secret),
            OwnerId = "v3owner",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Scopes = [Scope.Paste, Scope.User]
        });

        var (self, scopes) = await _authService.GetSelfWithScopesAsync(
            ContextWithAuthHeader("Bearer v3tok123-" + secret), CancellationToken.None);

        Assert.That(self, Is.Not.Null);
        Assert.That(self.Id, Is.EqualTo("v3owner"));
        Assert.That(scopes, Is.EquivalentTo(new[] { Scope.Paste, Scope.User }));
    }

    [Test]
    public async Task GetSelf_AcceptsMigratedV2Token_WithNoBearerPrefixAndNullExpiry()
    {
        // Mirrors exactly what PasteMyst.Migrator writes for a v2 api key: the whole key hashed
        // with SHA512, no expiry, scopes [Paste, User]. This end-to-end round-trip also proves the
        // migrator's hashing and AuthService's v2-fallback hashing stay in agreement.
        await SeedUser("v2owner");

        const string v2Key = "plainv2apikeywithnodashes";
        await _databaseFixture.MongoService.AccessTokens.InsertOneAsync(new AccessToken
        {
            Id = "v2keyid1",
            Description = "v2 api key",
            Token = Sha512Hex(v2Key),
            OwnerId = "v2owner",
            ExpiresAt = null,
            Scopes = [Scope.Paste, Scope.User]
        });

        // v2 clients send the raw key with no "Bearer " prefix.
        var (self, scopes) = await _authService.GetSelfWithScopesAsync(
            ContextWithAuthHeader(v2Key), CancellationToken.None);

        Assert.That(self, Is.Not.Null);
        Assert.That(self.Id, Is.EqualTo("v2owner"));
        Assert.That(scopes, Is.EquivalentTo(new[] { Scope.Paste, Scope.User }));
    }

    [Test]
    public async Task GetSelf_RejectsUnknownToken()
    {
        var (self, scopes) = await _authService.GetSelfWithScopesAsync(
            ContextWithAuthHeader("Bearer notreal1-doesnotexist"), CancellationToken.None);

        Assert.That(self, Is.Null);
        Assert.That(scopes, Is.Empty);
    }

    [Test]
    public async Task GetSelf_RejectsExpiredV3Token_AndDeletesIt()
    {
        await SeedUser("expiredowner");

        const string secret = "expiredsecret";
        await _databaseFixture.MongoService.AccessTokens.InsertOneAsync(new AccessToken
        {
            Id = "expired1",
            Token = Sha512Hex(secret),
            OwnerId = "expiredowner",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            Scopes = [Scope.Paste, Scope.User]
        });

        var (self, scopes) = await _authService.GetSelfWithScopesAsync(
            ContextWithAuthHeader("Bearer expired1-" + secret), CancellationToken.None);

        Assert.That(self, Is.Null);
        Assert.That(scopes, Is.Empty);

        var stillThere = await _databaseFixture.MongoService.AccessTokens
            .Find(a => a.Id == "expired1").FirstOrDefaultAsync();
        Assert.That(stillThere, Is.Null, "Expired token should be deleted on validation.");
    }

    [Test]
    public void HasScope_GrantsOnlyContainedScopes()
    {
        // Scope gating (used by controllers) is UserContext.HasScope: true if the token holds ANY of
        // the requested scopes. A Paste-only token must not satisfy a User-scoped action.
        var ctx = new UserContext();
        ctx.LoginUser(new User { Id = "1" }, [Scope.Paste]);

        Assert.Multiple(() =>
        {
            Assert.That(ctx.HasScope(Scope.Paste), Is.True);
            Assert.That(ctx.HasScope(Scope.User), Is.False);
            Assert.That(ctx.HasScope(Scope.UserAccessTokens), Is.False);
            Assert.That(ctx.HasScope(Scope.Paste, Scope.User), Is.True, "any-match semantics");
        });
    }
}
