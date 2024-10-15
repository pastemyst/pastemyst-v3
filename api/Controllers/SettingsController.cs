using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using pastemyst.Models;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/settings")]
public class SettingsController(SettingsService settingsService) : ControllerBase
{
    [HttpGet]
    public async Task<Settings> GetSettings()
    {
        return await settingsService.GetSettingsAsync(HttpContext);
    }

    [HttpPatch]
    public async Task UpdateSettings([FromBody] Settings settings)
    {
        await settingsService.UpdateSettingsAsync(HttpContext, settings);
    }

    [HttpGet("user")]
    public UserSettings GetUserSettings()
    {
        return settingsService.GetUserSettings();
    }

    [HttpPatch("user")]
    public async Task UpdateUserSettings([FromBody] UserSettings userSettings)
    {
        await settingsService.UpdateUserSettingsAsync(userSettings);
    }

    [HttpPatch("username")]
    public async Task SetUsername([FromBody] SetUsernameRequest setUsernameRequest)
    {
        await settingsService.SetUsernameAsync(setUsernameRequest.Username);
    }

    [HttpPatch("avatar")]
    public async Task SetAvatar([Required] IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var bytes = new byte[file.Length];
        _ = await stream.ReadAsync(bytes, 0, (int)file.Length);
        await settingsService.SetAvatarAsync(bytes, file.ContentType);
    }
}
