module pastemyst.services.auth_service;

import pastemyst.models;
import pastemyst.services;
import vibe.d;

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

    ///
    public string clientId;

    ///
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

/**
 * Services used for handling anything OAuth related.
 */
public class AuthService
{
    ///
    public const OAuthProvider githubProvider;

    ///
    public this(ConfigService config)
    {
        githubProvider = OAuthProvider("GitHub", config.github.clientId, config.github.clientSecret,
            "https://github.com/login/oauth/authorize", "https://github.com/login/oauth/access_token",
            "https://api.github.com/user", config.host ~ "api/v3/auth-web/login/github-callback",
            ["read:user"], "id", "login", "avatar_url");
    }

    /**
     * Returns the full authorization url for the provider.
     */
    public string getAuthorizationUrl(const OAuthProvider provider, string state) const
    {
        import std.array : join;
        import std.uri : encodeComponent;

        const scopes = provider.scopes.join(",");

        return provider.authorizationUrl ~
            "?client_id=" ~ provider.clientId ~
            "&scope=" ~ scopes ~
            "&redirect_uri=" ~ encodeComponent(provider.redirectUrl) ~
            "&state=" ~ state;
    }

    /**
     * Returns the access token from the provided code.
     */
    public string getAccessToken(const OAuthProvider provider, string code) const
    {
        const accessTokenUrl = provider.accessTokenUrl ~
                               "?client_id=" ~ provider.clientId ~
                               "&client_secret=" ~ provider.clientSecret ~
                               "&code=" ~ code;

        string accessToken;

        requestHTTP(accessTokenUrl,
            (scope req)
            {
                req.headers.addField("Accept", "application/json");
                req.method = HTTPMethod.POST;
            },
            (scope res)
            {
                if (res.statusCode != HTTPStatus.ok)
                {
                    logError("Failed reading the access token. Error while making a request to %s. Got response: %d.",
                        provider.name, res.statusCode);
                    throw new HTTPStatusException(HTTPStatus.internalServerError, "Failed reading the access token.");
                }

                try
                {
                    accessToken = parseJsonString(res.bodyReader.readAllUTF8())["access_token"].get!string();
                }
                catch (Exception e)
                {
                    logError("Failed reading the access token. Exception while parsing response JSON from %s.\n%s.",
                        provider.name, e);
                    throw new HTTPStatusException(HTTPStatus.internalServerError, "Failed reading the access token.");
                }
            });

        if (accessToken == "")
        {
            logError("Failed reading the access token. The resulting access token string is empty.");
            throw new HTTPStatusException(HTTPStatus.internalServerError, "Failed reading the access token.");
        }

        return accessToken;
    }

    /**
     * Returns the OAUth provider user from the provided access token.
     */
    public ProviderUser getProviderUser(const OAuthProvider provider,  const string accessToken) const
    {
        import std.conv : to;

        ProviderUser user;

        requestHTTP(provider.userInfoUrl,
            (scope req)
            {
                if (provider == githubProvider)
                {
                    req.headers.addField("Accept", "application/vnd.github.v3+json");
                    req.headers.addField("Authorization", "token " ~ accessToken);
                }
                else
                {
                    req.headers.addField("Accept", "application/json");
                    req.headers.addField("Authorization", "Bearer " ~ accessToken);
                }
            },
            (scope res)
            {
                if (res.statusCode != HTTPStatus.ok)
                {
                    logError("Failed getting the %s user. Response status: %d.",
                        provider.name, res.statusCode);
                    throw new HTTPStatusException(HTTPStatus.internalServerError,
                        "Failed getting the " ~ provider.name ~ " user.");
                }

                try
                {
                    Json json = parseJsonString(res.bodyReader.readAllUTF8());

                    user.id = json[provider.idJsonField].get!long().to!string();
                    user.username = json[provider.usernameJsonField].get!string();
                    user.avatarUrl = json[provider.avatarUrlJsonField].get!string();
                }
                catch (Exception e)
                {
                    logError("Failed getting the %s user. Exception: %s.",
                        provider.name, e);
                    throw new HTTPStatusException(HTTPStatus.internalServerError,
                        "Failed getting the " ~ provider.name ~ " user.");
                }
            });

        if (user == ProviderUser.init)
        {
            logError("Failed getting the %s user. The provider user is empty.",
                provider.name);
            throw new HTTPStatusException(HTTPStatus.internalServerError,
                "Failed getting the " ~ provider.name ~ " user.");
        }

        return user;
    }
}
