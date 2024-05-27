using System.Net;
using System.Web;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using MongoDB.Driver;
using pastemyst.Exceptions;
using pastemyst.Models;

// disable warning for using HMACSHA512Algorithm
#pragma warning disable CS0618

namespace pastemyst.Services;

public interface IAuthService
{
    public Task<string> InitiateLoginFlowAsync(string provider, HttpContext httpContext);

    public Task<string> HandleCallbackAsync(string provider, string state, string code, HttpContext httpContext);

    public Task RegisterUserAsync(string username, HttpContext httpContext);

    public Task<User> GetSelfAsync(HttpContext httpContext);

    public string Logout(HttpContext httpContext);
}

public class AuthService(
    IIdProvider idProvider,
    IOAuthService oAuthService,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IImageService imageService,
    IActionLogger actionLogger,
    IMongoService mongo)
    : IAuthService
{
    public async Task<string> InitiateLoginFlowAsync(string provider, HttpContext httpContext)
    {
        // Random state string used to validate that the acquired token was requested from here.
        var state = idProvider.GenerateId();

        httpContext.Session.SetString("state", state);
        await httpContext.Session.CommitAsync();

        var oauthProvider = oAuthService.OAuthProviders[provider];

        return oauthProvider.AuthUrl +
               "?client_id=" + oauthProvider.ClientId +
               "&redirect_uri=" + HttpUtility.UrlEncode(oauthProvider.RedirectUrl) +
               "&scope=" + string.Join(",", oauthProvider.Scopes) +
               "&response_type=code" +
               "&state=" + state;
    }

    public async Task<string> HandleCallbackAsync(string provider, string state, string code, HttpContext httpContext)
    {
        // Get the state string to validate the request
        var sessionState = httpContext.Session.GetString("state");

        if (sessionState is null)
        {
            throw new HttpException(HttpStatusCode.InternalServerError, "Missing state session.");
        }

        httpContext.Session.Clear();
        await httpContext.Session.CommitAsync();

        if (state != sessionState)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "The OAuth states don't match.");
        }

        var oAuthProvider = oAuthService.OAuthProviders[provider];
        var accessToken = await oAuthService.ExchangeTokenAsync(oAuthProvider, code);
        var providerUser = await oAuthService.GetProviderUserAsync(oAuthProvider, accessToken);

        var userFilter = Builders<User>.Filter.Eq(u => u.ProviderName, oAuthProvider.Name) &
                         Builders<User>.Filter.Eq(u => u.ProviderId, providerUser.Id);

        var existingUser = await mongo.Users.Find(userFilter).FirstOrDefaultAsync();

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

            var jwtToken = JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA512Algorithm())
                .AddClaim("exp", cookieExpirationTime.ToUnixTimeSeconds())
                .AddClaim("id", existingUser.Id)
                .AddClaim("username", existingUser.Username)
                .WithSecret(configuration["JwtSecret"])
                .Encode();

            cookie.Expires = cookieExpirationTime;

            httpContext.Response.Cookies.Append("pastemyst", jwtToken, cookie);

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

    public async Task RegisterUserAsync(string username, HttpContext httpContext)
    {
        var cookie = httpContext.Request.Cookies["pastemyst-registration"];

        if (cookie is null)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Missing the registration cookie.");
        }

        var usernameFilter = Builders<User>.Filter.Eq(u => u.Username, username);
        var existingUser = await mongo.Users.Find(usernameFilter).FirstOrDefaultAsync();

        if (existingUser is not null)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Username is already taken.");
        }

        var claims = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA512Algorithm())
            .WithSecret(configuration["JwtSecret"])
            .MustVerifySignature()
            .Decode<Dictionary<string, object>>(cookie);

        var id = await idProvider.GenerateId(async id => await mongo.Users.Find(u => u.Id == id).FirstOrDefaultAsync() is not null);

        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync((string)claims["avatarUrl"]);
        var imageBytes = await response.Content.ReadAsByteArrayAsync();

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
            Settings = new UserSettings()
        };

        await mongo.Users.InsertOneAsync(user);

        httpContext.Response.Cookies.Delete("pastemyst-registration");

        var jwtToken = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA512Algorithm())
            .AddClaim("exp", DateTimeOffset.Now.AddDays(30).ToUnixTimeSeconds())
            .AddClaim("id", user.Id)
            .AddClaim("username", user.Username)
            .WithSecret(configuration["JwtSecret"])
            .Encode();

        var newCookie = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.Now.AddDays(30),
            Secure = configuration.GetValue<bool>("Https")
        };

        httpContext.Response.Cookies.Append("pastemyst", jwtToken, newCookie);

        await actionLogger.LogActionAsync(ActionLogType.UserCreated, user.Id);
    }

    public async Task<User> GetSelfAsync(HttpContext httpContext)
    {
        var jwtToken = httpContext.Request.Cookies["pastemyst"];

        if (jwtToken is null)
        {
            string authHeader = httpContext.Request.Headers["Authorization"];

            if (authHeader is null || authHeader.Length <= "Bearer ".Length) return null;

            jwtToken = authHeader["Bearer ".Length..];
        }

        Dictionary<string, object> claims;
        try
        {
            claims = JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA512Algorithm())
                .WithSecret(configuration["JwtSecret"])
                .MustVerifySignature()
                .Decode<Dictionary<string, object>>(jwtToken);
        }
        catch (TokenExpiredException)
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "JWT token has expired.");
        }

        var userId = (string)claims["id"];

        return await mongo.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
    }

    public string Logout(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete("pastemyst");

        return configuration["ClientUrl"];
    }
}
