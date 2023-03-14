using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using pastemyst.Models.Settings;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/settings")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpPatch("username")]
    public async Task SetUsername([FromBody] SetUsernameRequest setUsernameRequest)
    {
        await _settingsService.SetUsernameAsync(setUsernameRequest.Username);
    }

    [HttpPatch("avatar")]
    public async Task SetAvatar([Required] IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var bytes = new byte[file.Length];
        _ = await stream.ReadAsync(bytes, 0, (int)file.Length);
        await _settingsService.SetAvatarAsync(bytes, file.ContentType);
    }
}