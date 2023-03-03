using Microsoft.AspNetCore.Mvc;
using pastemyst.Models;
using pastemyst.Models.Auth;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("login/{provider}")]
    public async Task<IActionResult> Login(string provider)
    {
        return Redirect(await _authService.InitiateLoginFlowAsync(provider, HttpContext));
    }

    [HttpGet("login/{provider}/callback")]
    public async Task<IActionResult> HandleCallback(string provider, [FromQuery] string state, [FromQuery] string code)
    {
        return Redirect(await _authService.HandleCallbackAsync(provider, state, code, HttpContext));
    }

    [HttpPost("auth/register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        await _authService.RegisterUserAsync(registerRequest.Username, HttpContext);
        return Ok();
    }

    [HttpGet("auth/self")]
    public async Task<User> GetSelf()
    {
        return await _authService.GetSelfAsync(HttpContext);
    }

    [HttpGet("auth/logout")]
    public IActionResult Logout()
    {
        return Redirect(_authService.Logout(HttpContext));
    }
}