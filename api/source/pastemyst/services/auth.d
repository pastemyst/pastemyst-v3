module pastemyst.services.auth;

import vibe.d;
import pastemyst.services;

@safe:

/**
 * Represents a single web service through which users can login using the OAuth flow.
 */
public struct OAuthProvider
{
    /**
     * Provider name.
     */
    public string name;

    public string clientId;

    public string clientSecret;

    /**
     * To be redirected to when logging in.
     */
    public string authorizationUrl;

    /**
     * Url from which an access token can be fetched.
     */
    public string accessTokenUrl;

    /**
     * Url from which user information can be fetched.
     */
    public string userInfoUrl;

    /**
     * Url to which the redirect will be made after authorization.
     */
    public string redirectUrl;

    /**
     * List of scopes that specify account access.
     */
    public string[] scopes;

    /**
     * Field name for the user ID. Used when reading user info from the service.
     */
    public string idJsonField;

    /**
     * Field name for the username. Used when reading user info from the service.
     */
    public string usernameJsonField;

    /**
     * Field name for the user avatar url. Used when reading user info from the service.
     */
    public string avatarUrlJsonField;
}

public class AuthService
{
    public const OAuthProvider githubProvider;

    public this(ConfigService config)
    {
        githubProvider = OAuthProvider("GitHub", config.github.clientId, config.github.clientId,
            "https://github.com/login/oauth/authorize", "https://github.com/login/oauth/access_token",
            "https://api.github.com/user", config.host ~ "api/v3/auth/github-callback",
            ["read:user"], "id", "login", "avatar_url");
    }

    /**
     * Returns the full authorization url for the provider.
     */
    public string getAuthorizationUrl(const OAuthProvider provider) const
    {
        import std.array : join;
        import std.uri : encodeComponent;

        const scopes = provider.scopes.join(",");

        return provider.authorizationUrl ~
            "?client_id=" ~ provider.clientId ~
            "&scope=" ~ scopes ~
            "&redirect_uri=" ~ encodeComponent(provider.redirectUrl) ~
            "&state=test_state";
    }
}
