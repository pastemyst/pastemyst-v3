using System.Net;
using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Extensions;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers;

[ApiController]
[Route("/api/v3")]
public sealed class AuthController(AuthService authService, UserContext userContext) : ControllerBase
{
    [HttpGet("login/{provider}")]
    public async Task<IActionResult> Login(string provider, CancellationToken cancellationToken)
    {
        return Redirect(await authService.InitiateLoginFlowAsync(provider, HttpContext, cancellationToken));
    }

    [HttpGet("login/{provider}/callback")]
    public async Task<IActionResult> HandleCallback(string provider, [FromQuery] string state, [FromQuery] string code, CancellationToken cancellationToken)
    {
        return Redirect(await authService.HandleCallbackAsync(provider, state, code, HttpContext, cancellationToken));
    }

    [HttpPost("auth/register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest, CancellationToken cancellationToken)
    {
        await authService.RegisterUserAsync(registerRequest.Username, HttpContext, cancellationToken);
        return Ok();
    }

    [HttpGet("auth/self")]
    public User GetSelf()
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to get self.");
        }

        if (!userContext.HasScope(Scope.User, Scope.UserRead))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.UserRead.ToEnumString()}.");
        }

        return userContext.Self;
    }

    [HttpPost("auth/self/access_tokens")]
    public async Task<GenerateAccessTokenResponse> GenerateAccessToken([FromBody] GenerateAccessTokenRequest accessTokenRequest, CancellationToken cancellationToken)
    {
        return await authService.GenerateAccessTokenForSelf(accessTokenRequest.Scopes, accessTokenRequest.ExpiresIn, accessTokenRequest.Description, cancellationToken);
    }

    [HttpGet("auth/self/access_tokens")]
    public async Task<List<AccessTokenResponse>> GetAccessTokens(CancellationToken cancellationToken)
    {
        return await authService.GetAccessTokensForSelf(cancellationToken);
    }

    [HttpDelete("auth/self/access_tokens/{id}")]
    public async Task DeleteAccessToken(string id, CancellationToken cancellationToken)
    {
        await authService.DeleteAccessTokenForSelf(id, cancellationToken);
    }

    [HttpGet("auth/logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        return Redirect(await authService.Logout(HttpContext, cancellationToken));
    }
}
