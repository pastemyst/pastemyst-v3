module pastemyst.controllers.auth;

import std.datetime;
import vibe.d;
import hunt.jwt;
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
    private ConfigService configService;

    public this(AuthService authService, UserService userService, ConfigService configService)
    {
        this.authService = authService;
        this.userService = userService;
        this.configService = configService;
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
    public void getGithubCallback(HTTPServerRequest req, HTTPServerResponse res, string code, string state) @trusted
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

        auto user = userService.findByProviderId(authService.githubProvider.name, providerUser.id);

        // create user if doesn't exists
        if (user.isNull())
        {
            user = User(null,
                        providerUser.username,
                        [authService.githubProvider.name: providerUser.id],
                        providerUser.avatarUrl);

            userService.createUser(user.get());
        }

        const timeInMonth = Clock.currTime() + 30.days;

        auto jwtToken = new JwtToken(JwtAlgorithm.HS512);
        jwtToken.claims.exp = timeInMonth.toUnixTime();
        jwtToken.claims.set("id", user.get().id);
        jwtToken.claims.set("username", user.get().username);

        string encodedToken = jwtToken.encode(configService.secret);

        // todo: make sure cookie is secure on https
        auto cookie = new Cookie();
        cookie.expire = dur!"days"(30);
        cookie.path = "/";
        cookie.value = encodedToken;
        cookie.sameSite(Cookie.SameSite.strict);

        res.cookies.addField("pastemyst", cookie);

        res.terminateSession();

        res.redirect("/");
    }
}
