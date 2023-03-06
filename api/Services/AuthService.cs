using System.Net;
using System.Web;
using JWT.Algorithms;
using JWT.Builder;
using pastemyst.DbContexts;
using pastemyst.Exceptions;
using pastemyst.Models;

namespace pastemyst.Services;

public interface IAuthService
{
    public Task<string> InitiateLoginFlowAsync(string provider, HttpContext httpContext);

    public Task<string> HandleCallbackAsync(string provider, string state, string code, HttpContext httpContext);

    public Task RegisterUserAsync(string username, HttpContext httpContext);

    public Task<User> GetSelfAsync(HttpContext httpContext);

    public string Logout(HttpContext httpContext);
}

public class AuthService : IAuthService
{
    private readonly IIdProvider _idProvider;
    private readonly IUserProvider _userProvider;
    private readonly IOAuthService _oAuthService;
    private readonly IImageService _imageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DataContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(IIdProvider idProvider, IOAuthService oAuthService, IUserProvider userProvider,
        IConfiguration configuration, IHttpClientFactory httpClientFactory, IImageService imageService,
        DataContext dbContext)
    {
        _idProvider = idProvider;
        _oAuthService = oAuthService;
        _userProvider = userProvider;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _imageService = imageService;
        _dbContext = dbContext;
    }

    public async Task<string> InitiateLoginFlowAsync(string provider, HttpContext httpContext)
    {
        // Random state string used to validate that the acquired token was requested from here.
        var state = _idProvider.GenerateId();

        httpContext.Session.SetString("state", state);
        await httpContext.Session.CommitAsync();

        var oauthProvider = _oAuthService.OAuthProviders[provider];

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

        var oAuthProvider = _oAuthService.OAuthProviders[provider];
        var accessToken = await _oAuthService.ExchangeTokenAsync(oAuthProvider, code);
        var providerUser = await _oAuthService.GetProviderUserAsync(oAuthProvider, accessToken);
        var existingUser = await _userProvider.FindByProviderAsync(oAuthProvider.Name, providerUser.Id);

        var cookie = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = _configuration.GetValue<bool>("Https")
        };

        if (existingUser is not null)
        {
            var cookieExpirationTime = DateTimeOffset.Now.AddDays(30);

            var jwtToken = JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA512Algorithm())
                .AddClaim("exp", cookieExpirationTime.ToUnixTimeSeconds())
                .AddClaim("id", existingUser.Id)
                .AddClaim("username", existingUser.Username)
                .WithSecret(_configuration["JwtSecret"])
                .Encode();

            cookie.Expires = cookieExpirationTime;

            httpContext.Response.Cookies.Append("pastemyst", jwtToken, cookie);

            return _configuration["ClientUrl"]!;
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
                .WithSecret(_configuration["JwtSecret"])
                .Encode();

            cookie.Expires = cookieExpirationTime;

            httpContext.Response.Cookies.Append("pastemyst-registration", jwtToken, cookie);

            return $"{_configuration["ClientUrl"]}/create-account?username={providerUser.Username}";
        }
    }

    public async Task RegisterUserAsync(string username, HttpContext httpContext)
    {
        var cookie = httpContext.Request.Cookies["pastemyst-registration"];

        if (cookie is null)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Missing the registration cookie.");
        }

        if (await _userProvider.ExistsByUsernameAsync(username))
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Username is already taken.");
        }

        var claims = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA512Algorithm())
            .WithSecret(_configuration["JwtSecret"])
            .MustVerifySignature()
            .Decode<Dictionary<string, object>>(cookie);

        var id = await _idProvider.GenerateId(async id => await _userProvider.ExistsById(id));

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync((string)claims["avatarUrl"]);
        var imageBytes = await response.Content.ReadAsByteArrayAsync();

        var avatar = await _imageService.UploadImageAsync(
            imageBytes,
            response.Content.Headers.ContentType?.MediaType ?? "image/png"
        );

        var user = new User
        {
            Id = id,
            CreatedAt = DateTime.UtcNow,
            Username = username,
            Avatar = avatar,
            ProviderName = (string)claims["providerName"],
            ProviderId = (string)claims["providerId"]
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        httpContext.Response.Cookies.Delete("pastemyst-registration");

        var jwtToken = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA512Algorithm())
            .AddClaim("exp", DateTimeOffset.Now.AddDays(30).ToUnixTimeSeconds())
            .AddClaim("id", user.Id)
            .AddClaim("username", user.Username)
            .WithSecret(_configuration["JwtSecret"])
            .Encode();

        var newCookie = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.Now.AddDays(30),
            Secure = _configuration.GetValue<bool>("Https")
        };

        httpContext.Response.Cookies.Append("pastemyst", jwtToken, newCookie);
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

        var claims = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA512Algorithm())
            .WithSecret(_configuration["JwtSecret"])
            .MustVerifySignature()
            .Decode<Dictionary<string, object>>(jwtToken);

        var userId = (string)claims["id"];

        var user = await _dbContext.Users.FindAsync(userId);

        return user;
    }

    public string Logout(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete("pastemyst");
        
        return _configuration["ClientUrl"];
    }
}