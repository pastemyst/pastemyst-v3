namespace pastemyst.Models.Auth;

public class OAuthProviderConfig
{
    public string Name { get; init; }

    public string ClientId { get; init; }

    public string ClientSecret { get; init; }

    public string AuthUrl { get; init; }

    public string TokenUrl { get; init; }

    public string RedirectUrl { get; init; }

    public string[] Scopes { get; init; }

    public string UserUrl { get; init; }

    public string IdJsonField { get; init; }

    public string UsernameJsonField { get; init; }

    public string AvatarUrlJsonField { get; init; }
}