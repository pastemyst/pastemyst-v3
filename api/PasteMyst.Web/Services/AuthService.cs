using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using JWT.Algorithms;
using JWT.Builder;
using MongoDB.Bson;
using MongoDB.Driver;
using PasteMyst.Web.Extensions;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Utils;

// disable warning for using HMACSHA512Algorithm
#pragma warning disable CS0618

namespace PasteMyst.Web.Services;

public class AuthService(
    IdProvider idProvider,
    OAuthService oAuthService,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ImageService imageService,
    ActionLogger actionLogger,
    UserContext userContext,
    MongoService mongo)
{
    public async Task<string> InitiateLoginFlowAsync(string provider, HttpContext httpContext, CancellationToken cancellationToken)
    {
        // Random state string used to validate that the acquired token was requested from here.
        var state = idProvider.GenerateId();

        httpContext.Session.SetString("state", state);
        await httpContext.Session.CommitAsync(cancellationToken);

        var oauthProvider = oAuthService.OAuthProviders[provider];

        return oauthProvider.AuthUrl +
               "?client_id=" + oauthProvider.ClientId +
               "&redirect_uri=" + HttpUtility.UrlEncode(oauthProvider.RedirectUrl) +
               "&scope=" + string.Join(",", oauthProvider.Scopes) +
               "&response_type=code" +
               "&state=" + state;
    }

    public async Task<string> HandleCallbackAsync(string provider, string state, string code, HttpContext httpContext, CancellationToken cancellationToken)
    {
        // Get the state string to validate the request
        var sessionState = httpContext.Session.GetString("state");

        if (sessionState is null)
        {
            throw new HttpException(HttpStatusCode.InternalServerError, "Missing state session.");
        }

        httpContext.Session.Clear();
        await httpContext.Session.CommitAsync(cancellationToken);

        if (state != sessionState)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "The OAuth states don't match.");
        }

        var oAuthProvider = oAuthService.OAuthProviders[provider];
        var accessToken = await oAuthService.ExchangeTokenAsync(oAuthProvider, code, cancellationToken);
        var providerUser = await oAuthService.GetProviderUserAsync(oAuthProvider, accessToken, cancellationToken);

        var userFilter = Builders<User>.Filter.Regex(u => u.ProviderName, new BsonRegularExpression(oAuthProvider.Name, "i")) &
                         Builders<User>.Filter.Eq(u => u.ProviderId, providerUser.Id);

        var existingUser = await mongo.Users.Find(userFilter).FirstOrDefaultAsync(cancellationToken);

        var cookie = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = configuration.GetValue<bool>("Https")
        };

        if (existingUser is not null)
        {
            var cookieExpirationTime = DateTimeOffset.Now.AddDays(30);

            var (newAccessToken, _) = await GenerateAccessToken(existingUser, [Scope.Paste, Scope.User, Scope.UserAccessTokens], ExpiresIn.OneMonth, cancellationToken, hidden: true);

            cookie.Expires = cookieExpirationTime;

            httpContext.Response.Cookies.Append("pastemyst", newAccessToken, cookie);

            return configuration["ClientUrl"]!;
        }
        else
        {
            var cookieExpirationTime = DateTimeOffset.Now.AddHours(1);

            var jwtToken = JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA512Algorithm())
                .AddClaim("exp", cookieExpirationTime.ToUnixTimeSeconds())
                .AddClaim("providerName", oAuthProvider.Name)
                .AddClaim("providerId", providerUser.Id)
                .AddClaim("avatarUrl", providerUser.AvatarUrl)
                .WithSecret(configuration["JwtSecret"])
                .Encode();

            cookie.Expires = cookieExpirationTime;

            httpContext.Response.Cookies.Append("pastemyst-registration", jwtToken, cookie);

            return $"{configuration["ClientUrl"]}/create-account?username={providerUser.Username}";
        }
    }

    public async Task RegisterUserAsync(string username, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var cookie = httpContext.Request.Cookies["pastemyst-registration"];

        if (cookie is null)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Missing the registration cookie.");
        }

        var usernameFilter = Builders<User>.Filter.Eq(u => u.Username, username);
        var existingUser = await mongo.Users.Find(usernameFilter).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (existingUser is not null)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Username is already taken.");
        }

        var claims = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA512Algorithm())
            .WithSecret(configuration["JwtSecret"])
            .MustVerifySignature()
            .Decode<Dictionary<string, object>>(cookie);

        var id = await idProvider.GenerateId(async id => await mongo.Users.Find(u => u.Id == id).FirstOrDefaultAsync(cancellationToken: cancellationToken) is not null);

        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync((string)claims["avatarUrl"], cancellationToken);
        var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        var avatarId = await imageService.UploadImageAsync(
            imageBytes,
            response.Content.Headers.ContentType?.MediaType ?? "image/png"
        );

        var user = new User
        {
            Id = id,
            CreatedAt = DateTime.UtcNow,
            Username = username,
            AvatarId = avatarId,
            ProviderName = (string)claims["providerName"],
            ProviderId = (string)claims["providerId"],
            UserSettings = new UserSettings(),
            Settings = new Settings()
        };

        await mongo.Users.InsertOneAsync(user, cancellationToken: cancellationToken);

        httpContext.Response.Cookies.Delete("pastemyst-registration");

        var (accessToken, _) = await GenerateAccessToken(user, [Scope.Paste, Scope.User, Scope.UserAccessTokens], ExpiresIn.OneMonth, cancellationToken, hidden: true);

        var newCookie = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.Now.AddDays(30),
            Secure = configuration.GetValue<bool>("Https")
        };

        httpContext.Response.Cookies.Append("pastemyst", accessToken, newCookie);

        await actionLogger.LogActionAsync(ActionLogType.UserCreated, user.Id);
    }

    public async Task<(User, Scope[])> GetSelfWithScopesAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var accessToken = httpContext.Request.Cookies["pastemyst"];

        if (accessToken is null)
        {
            string authHeader = httpContext.Request.Headers.Authorization;

            if (authHeader is null || authHeader.Length <= "Bearer ".Length) return (null, []);

            accessToken = authHeader["Bearer ".Length..];
        }

        var (valid, _, userId, scopes) = await AccessTokenValid(accessToken);

        return valid ? (await mongo.Users.Find(u => u.Id == userId).FirstOrDefaultAsync(cancellationToken: cancellationToken), scopes) : (null, []);
    }

    public async Task<string> Logout(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var accessToken = httpContext.Request.Cookies["pastemyst"];

        var (valid, accessTokenId, userId, _) = await AccessTokenValid(accessToken);

        if (!valid)
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "Access token is not valid.");
        }

        httpContext.Response.Cookies.Delete("pastemyst");

        var filter = Builders<AccessToken>.Filter.Eq(a => a.Id, accessTokenId);
        await mongo.AccessTokens.DeleteOneAsync(filter, cancellationToken);

        await actionLogger.LogActionAsync(ActionLogType.AccessTokenDeleted, userId);

        return configuration["ClientUrl"];
    }

    private async Task<(string, DateTime?)> GenerateAccessToken(User owner, Scope[] scopes, ExpiresIn expiresIn, CancellationToken cancellationToken, bool hidden = false, string description = "")
    {
        var secureString = RandomNumberGenerator.GetHexString(64, true);
        var hashedToken = SHA512.HashData(Encoding.UTF8.GetBytes(secureString));

        var hashStringBuilder = new StringBuilder();
        foreach (var b in hashedToken)
        {
            hashStringBuilder.Append(b.ToString("x2"));
        }

        var accessToken = new AccessToken
        {
            Id = await idProvider.GenerateId(AccessTokenExistsById),
            Token = hashStringBuilder.ToString(),
            Scopes = scopes,
            OwnerId = owner.Id,
            ExpiresAt = ExpiresInUtils.ToDeletesAt(DateTime.UtcNow, expiresIn),
            Hidden = hidden,
            Description = description
        };

        await mongo.AccessTokens.InsertOneAsync(accessToken, cancellationToken: cancellationToken);

        await actionLogger.LogActionAsync(ActionLogType.AccessTokenCreated, owner.Id);

        return ($"{accessToken.Id}-{secureString}", accessToken.ExpiresAt);
    }

    public async Task<GenerateAccessTokenResponse> GenerateAccessTokenForSelf(Scope[] scopes, ExpiresIn expiresIn,
        string description, CancellationToken cancellationToken)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Forbidden, "You must be authorized to generate new access tokens.");
        }

        if (!userContext.HasScope(Scope.UserAccessTokens))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.UserAccessTokens.ToEnumString()}.");
        }

        var (accessToken, expiresAt) = await GenerateAccessToken(userContext.Self, scopes, expiresIn, cancellationToken, hidden: false, description);

        return new GenerateAccessTokenResponse { AccessToken = accessToken, ExpiresAt = expiresAt };
    }

    public async Task<List<AccessTokenResponse>> GetAccessTokensForSelf(CancellationToken cancellationToken)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Forbidden, "You must be authorized to generate new access tokens.");
        }

        if (!userContext.HasScope(Scope.UserAccessTokens))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.UserAccessTokens.ToEnumString()}.");
        }

        var filter = Builders<AccessToken>.Filter.Eq(a => a.Hidden, false) & Builders<AccessToken>.Filter.Eq(a => a.OwnerId, userContext.Self.Id);
        var accessTokens = (await mongo.AccessTokens.FindAsync(filter, cancellationToken: cancellationToken)).ToList(cancellationToken: cancellationToken).Select(a => new AccessTokenResponse
                {
                    Id = a.Id,
                    Description = a.Description,
                    CreatedAt = a.CreatedAt,
                    ExpiresAt = a.ExpiresAt,
                    Scopes = a.Scopes
                });

        return accessTokens.ToList();
    }

    public async Task DeleteAccessTokenForSelf(string accessTokenId, CancellationToken cancellationToken)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Forbidden, "You must be authorized to generate new access tokens.");
        }

        if (!userContext.HasScope(Scope.UserAccessTokens))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.UserAccessTokens.ToEnumString()}.");
        }

        var accessToken = await mongo.AccessTokens.Find(a => a.Id == accessTokenId).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (accessToken is null)
        {
            throw new HttpException(HttpStatusCode.NotFound, "Access token not found.");
        }

        await mongo.AccessTokens.DeleteOneAsync(a => a.Id == accessTokenId, cancellationToken: cancellationToken);
    }

    private async Task<(bool, string, string, Scope[])> AccessTokenValid(String accessToken)
    {
        var splitted = accessToken.Split("-");
        var accessTokenId = splitted[0];
        var accessTokenRaw = splitted[1];

        var accessTokenDb = await mongo.AccessTokens.Find(a => a.Id == accessTokenId).FirstOrDefaultAsync();

        if (accessTokenDb is null) return (false, null, null, []);

        if (accessTokenDb.ExpiresAt <= DateTime.UtcNow)
        {
            await mongo.AccessTokens.DeleteOneAsync(a => a.Id == accessTokenId);
            return (false, null, null, []);
        }

        using var sha = SHA512.Create();
        var hashedToken = sha.ComputeHash(Encoding.UTF8.GetBytes(accessTokenRaw));

        var hashStringBuilder = new StringBuilder();
        foreach (byte b in hashedToken)
        {
            hashStringBuilder.Append(b.ToString("x2"));
        }

        if (!accessTokenDb.Token.Equals(hashStringBuilder.ToString())) return (false, null, null, []);

        return (true, accessTokenDb.Id, accessTokenDb.OwnerId, accessTokenDb.Scopes);
    }

    private async Task<bool> AccessTokenExistsById(string id)
    {
        return await mongo.AccessTokens.Find(a => a.Id == id).FirstOrDefaultAsync() is not null;
    }
}
