using Microsoft.AspNetCore.Mvc;
using pastemyst.Models.Auth;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpGet("login/{provider}")]
    public async Task<IActionResult> Login(string provider)
    {
        return Redirect(await authService.InitiateLoginFlowAsync(provider, HttpContext));
    }

    [HttpGet("login/{provider}/callback")]
    public async Task<IActionResult> HandleCallback(string provider, [FromQuery] string state, [FromQuery] string code)
    {
        return Redirect(await authService.HandleCallbackAsync(provider, state, code, HttpContext));
    }

    [HttpPost("auth/register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        await authService.RegisterUserAsync(registerRequest.Username, HttpContext);
        return Ok();
    }

    [HttpGet("auth/self")]
    public async Task<IActionResult> GetSelf()
    {
        var self = await authService.GetSelfAsync(HttpContext);

        if (self is null) return Unauthorized();
        return Ok(self);
    }

    [HttpGet("auth/logout")]
    public IActionResult Logout()
    {
        return Redirect(authService.Logout(HttpContext));
    }
}
