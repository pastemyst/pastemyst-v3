module pastemyst.controllers.auth;

import vibe.d;
import pastemyst.services;

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

    public this(AuthService authService)
    {
        this.authService = authService;
    }

    @path("login/github")
    public void getGitHubLogin() @safe
    {
        redirect(authService.getAuthorizationUrl(authService.githubProvider));
    }
}
