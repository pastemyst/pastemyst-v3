module pastemyst.controllers.auth_controller;

import jwt.algorithms;
import jwt.jwt;
import pastemyst.models;
import pastemyst.serialization;
import pastemyst.services;
import std.algorithm;
import std.datetime;
import vibe.d;

/**
 * API /api/v3/auth
 */
@path("/api/v3/auth")
@serializationPolicy!(UserPolicy)
public interface IAuthController
{
    /**
     * POST /api/v3/auth/register
     *
     * Creates a new account.
     */
    @headerParam("authorization", "Authorization")
    @bodyParam("username", "username")
    Json postRegister(string authorization, string username) @safe;

    /**
     * GET /api/v3/auth/self
     *
     * Returns the authorized user from the provided token.
     */
    @headerParam("authorization", "Authorization")
    User getSelf(string authorization) @safe;
}

/**
 * API /api/v3/auth
 */
public class AuthController : IAuthController
{
    private ConfigService configService;
    private UserService userService;

    ///
    public this(ConfigService configService, UserService userService)
    {
        this.configService = configService;
        this.userService = userService;
    }

    public override Json postRegister(string authorization, string username) @trusted
    {
        enforceHTTP(authorization.startsWith("Bearer "), HTTPStatus.badRequest,
            "Invalid authorization scheme. The token must be provided as a Bearer token.");

        const usernameErr = userService.validateUsername(username);

        enforceHTTP(usernameErr is null, HTTPStatus.badRequest, usernameErr);

        const encodedToken = authorization["Bearer ".length..$];

        string providerName;
        string providerId;
        string avatarUrl;

        try
        {
            auto token = verify(encodedToken, configService.jwtSecret, [JWTAlgorithm.HS512]);

            providerName = token.claims.get("provider");
            providerId = token.claims.get("id");
            avatarUrl = token.claims.get("avatarUrl");
        }
        catch (Exception e)
        {
            logError("User tried to register with an invalid token. Exception: %s", e);
            throw new HTTPStatusException(HTTPStatus.badRequest, "Provided token is not valid.");
        }

        enforceHTTP(!userService.existsByProviderId(providerName, providerId), HTTPStatus.badRequest,
            "A user already exists with the same provider.");

        User user = {
            username: username,
            [providerName: providerId],
            avatarUrl: avatarUrl
        };

        userService.createUser(user);

        const timeInMonth = Clock.currTime() + 30.days;
        auto jwtToken = new Token(JWTAlgorithm.HS512);
        jwtToken.claims.exp = timeInMonth.toUnixTime();
        jwtToken.claims.set("id", user.id);
        jwtToken.claims.set("username", user.username);

        return Json(["token": Json(jwtToken.encode(configService.jwtSecret))]);
    }

    public override User getSelf(string authorization) @trusted
    {
        enforceHTTP(authorization.startsWith("Bearer "), HTTPStatus.badRequest,
            "Invalid authorization scheme. The token must be provided as a Bearer token.");

        const encodedToken = authorization["Bearer ".length..$];

        string id;

        try
        {
            auto token = verify(encodedToken, configService.jwtSecret, [JWTAlgorithm.HS512]);

            id = token.claims.get("id");
        }
        catch (Exception e)
        {
            logError("User tried calling an auth endpoint with an invalid token. Exception: %s", e);
            throw new HTTPStatusException(HTTPStatus.badRequest, "Provided token is notr valid.");
        }

        return userService.findById(id).get();
    }
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

    ///
    public this(AuthService authService, UserService userService, ConfigService configService)
    {
        this.authService = authService;
        this.userService = userService;
        this.configService = configService;
    }

    /**
     * /api/v3/auth-web/login/github
     *
     * Initiates logging in with github.
     */
    @path("login/github")
    public void getGitHubLogin(HTTPServerResponse res) @safe
    {
        import std.algorithm : filter, map;
        import std.array : appender;
        import std.ascii : isAlphaNum;
        import std.base64 : Base64;
        import std.random : rndGen;
        import std.range : take;

        // generate a random state string for oauth
        auto rndNums = rndGen().map!(a => cast(ubyte) a)().take(32);
        auto apndr = appender!string();
        Base64.encode(rndNums, apndr);
        auto state = apndr.data.filter!isAlphaNum().to!string();

        auto session = res.startSession();
        session.set("oauth_state", state);

        res.redirect(authService.getAuthorizationUrl(authService.githubProvider, state));
    }

    /**
     * This is an OAauth callback from github.
     */
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

        auto jwtToken = new Token(JWTAlgorithm.HS512);

        // todo: make sure cookie is secure on https
        auto cookie = new Cookie();
        cookie.path = "/";
        cookie.httpOnly = false;
        cookie.sameSite(Cookie.SameSite.strict);

        // terminate the session that was only used for storing the OAuth state string
        res.terminateSession();

        // if user doesn't exist, create a jwt token that is used for registration purposes only
        if (user.isNull())
        {
            const timeInHour = Clock.currTime() + 1.hours;
            jwtToken.claims.exp = timeInHour.toUnixTime();

            jwtToken.claims.set("id", providerUser.id);
            jwtToken.claims.set("provider", authService.githubProvider.name);
            jwtToken.claims.set("avatarUrl", providerUser.avatarUrl);

            cookie.expire = dur!"hours"(1);
            cookie.value = jwtToken.encode(configService.jwtSecret);

            res.cookies.addField("pastemyst-registration", cookie);

            res.redirect(configService.clientHost ~ "create-account?username=" ~ providerUser.username);
        }
        else
        {
            const timeInMonth = Clock.currTime() + 30.days;
            jwtToken.claims.exp = timeInMonth.toUnixTime();
            jwtToken.claims.set("id", user.get().id);
            jwtToken.claims.set("username", user.get().username);

            cookie.expire = dur!"days"(30);
            cookie.value = jwtToken.encode(configService.jwtSecret);

            res.cookies.addField("pastemyst", cookie);

            res.redirect(configService.clientHost);
        }
    }
}
