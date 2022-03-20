module pastemyst.controllers.auth;

import vibe.d;
import pastemyst.services;
import pastemyst.models;

@path("/api/v3/auth")
public interface IAuthController
{

}

public class AuthController : IAuthController
{

}

/**
 * A web controller, not API. Responsible for initializing OAuth provider login.
 */
@path("/api/v3/auth-web")
public class AuthWebController
{
    private const AuthService authService;
    private UserService userService;

    public this(AuthService authService, UserService userService)
    {
        this.authService = authService;
        this.userService = userService;
    }

    @path("login/github")
    public void getGitHubLogin(HTTPServerResponse res) @safe
    {
        import std.random : rndGen;
        import std.algorithm : map, filter;
        import std.range : take;
        import std.array : appender;
        import std.base64 : Base64;
        import std.ascii : isAlphaNum;

        // generate a random state string for oauth
        auto rndNums = rndGen().map!(a => cast(ubyte) a)().take(32);
        auto apndr = appender!string();
        Base64.encode(rndNums, apndr);
        auto state = apndr.data.filter!isAlphaNum().to!string();

        auto session = res.startSession();
        session.set("oauth_state", state);

        res.redirect(authService.getAuthorizationUrl(authService.githubProvider, state));
    }

    @path("/login/github-callback")
    public void getGithubCallback(HTTPServerRequest req, HTTPServerResponse res, string code, string state) @safe
    {
        if (!req.session)
        {
            throw new HTTPStatusException(HTTPStatus.badRequest,
                "OAuth callback called but the session hasn't been started.");
        }

        const sessionState = req.session.get!string("oauth_state");

        if (state != sessionState) throw new HTTPStatusException(HTTPStatus.badRequest, "Invalid state code.");

        const accessToken = authService.getAccessToken(authService.githubProvider, code);

        const providerUser = authService.getProviderUser(authService.githubProvider, accessToken);

        if (userService.existsByProviderId(authService.githubProvider.name, providerUser.id))
        {
            logInfo("User already exists");
        }
        else
        {
            User newUser = {
                username: providerUser.username,
                avatarUrl: providerUser.avatarUrl,
                oauthProviderIds: [authService.githubProvider.name: providerUser.id]
            };

            userService.createUser(newUser);
        }

        res.terminateSession();

        res.redirect("/");
    }
}
