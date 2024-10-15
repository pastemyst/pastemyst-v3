using System.Net;
using System.Text.Json;
using System.Web;
using pastemyst.Exceptions;
using pastemyst.Models.Auth;

namespace pastemyst.Services;

public class OAuthService
{
    public Dictionary<string, OAuthProviderConfig> OAuthProviders { get; }

    private OAuthProviderConfig GitHubProvider { get; }
    private OAuthProviderConfig GitLabProvider { get; }

    private readonly ILogger<OAuthService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public OAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory,
        ILogger<OAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        GitHubProvider = new OAuthProviderConfig
        {
            ClientId = configuration["GitHub:ClientId"]!,
            ClientSecret = configuration["GitHub:ClientSecret"]!,
            AuthUrl = "https://github.com/login/oauth/authorize",
            TokenUrl = "https://github.com/login/oauth/access_token",
            RedirectUrl = configuration["Host"] + "/api/v3/login/github/callback",
            Scopes = ["read:user"],
            Name = "GitHub",
            UserUrl = "https://api.github.com/user",
            IdJsonField = "id",
            UsernameJsonField = "login",
            AvatarUrlJsonField = "avatar_url"
        };

        GitLabProvider = new OAuthProviderConfig
        {
            ClientId = configuration["GitLab:ClientId"]!,
            ClientSecret = configuration["GitLab:ClientSecret"]!,
            AuthUrl = "https://gitlab.com/oauth/authorize",
            TokenUrl = "https://gitlab.com/oauth/token",
            RedirectUrl = configuration["Host"] + "/api/v3/login/gitlab/callback",
            Scopes = ["read_user"],
            Name = "GitLab",
            UserUrl = "https://gitlab.com/api/v4/user",
            IdJsonField = "id",
            UsernameJsonField = "username",
            AvatarUrlJsonField = "avatar_url"
        };

        OAuthProviders = new Dictionary<string, OAuthProviderConfig>
        {
            { "github", GitHubProvider },
            { "gitlab", GitLabProvider }
        };
    }

    public async Task<OAuthProviderUser> GetProviderUserAsync(OAuthProviderConfig provider, string token)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, provider.UserUrl);

        requestMessage.Headers.Add("User-Agent", "pastemyst");

        if (provider.Equals(GitHubProvider))
        {
            requestMessage.Headers.Add("Accept", "application/vnd.github.v3+json");
            requestMessage.Headers.Add("Authorization", "token " + token);
        }
        else
        {
            requestMessage.Headers.Add("Accept", "application/json");
            requestMessage.Headers.Add("Authorization", "Bearer " + token);
        }

        var response = await httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed getting OAuth user, " +
                             $"status {response.StatusCode}, " +
                             $"message: {await response.Content.ReadAsStringAsync()}");
            throw new HttpException(HttpStatusCode.InternalServerError, "Failed getting OAuth user.");
        }

        await using var contentStream = await response.Content.ReadAsStreamAsync();

        var responseObject = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(contentStream);

        return new OAuthProviderUser
        {
            Id = responseObject![provider.IdJsonField].ToString(),
            Username = responseObject[provider.UsernameJsonField].GetString()!,
            AvatarUrl = responseObject[provider.AvatarUrlJsonField].GetString()!
        };
    }

    public async Task<string> ExchangeTokenAsync(OAuthProviderConfig provider, string code)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetProviderTokenUrl(provider, code));

        requestMessage.Headers.Add("Accept", "application/json");

        var response = await httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpException(HttpStatusCode.InternalServerError, "Failed getting the access token.");
        }

        await using var contentStream = await response.Content.ReadAsStreamAsync();

        var responseObject = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(contentStream);

        var accessToken = responseObject?["access_token"].GetString();

        if (accessToken is null)
        {
            throw new HttpException(HttpStatusCode.InternalServerError, "Missing access token.");
        }

        return accessToken;
    }

    private string GetProviderTokenUrl(OAuthProviderConfig provider, string code)
    {
        return provider.TokenUrl +
               "?client_id=" + provider.ClientId +
               "&client_secret=" + provider.ClientSecret +
               "&grant_type=authorization_code" +
               "&redirect_uri=" + HttpUtility.UrlEncode(provider.RedirectUrl) +
               "&code=" + code;
    }
}
