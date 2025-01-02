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
        return await settingsService.GetSettingsAsync(HttpContext, HttpContext.User);
    }

    [HttpPatch]
    public async Task UpdateSettings([FromBody] Settings settings)
    {
        await settingsService.UpdateSettingsAsync(HttpContext, HttpContext.User, settings);
    }

    [HttpGet("user")]
    public async Task<UserSettings> GetUserSettings()
    {
        return await settingsService.GetUserSettingsAsync(HttpContext.User);
    }

    [HttpPatch("user")]
    public async Task UpdateUserSettings([FromBody] UserSettings userSettings)
    {
        await settingsService.UpdateUserSettingsAsync(HttpContext.User, userSettings);
    }

    [HttpPatch("username")]
    public async Task SetUsername([FromBody] SetUsernameRequest setUsernameRequest)
    {
        await settingsService.SetUsernameAsync(HttpContext.User, setUsernameRequest.Username);
    }

    [HttpPatch("avatar")]
    public async Task SetAvatar([Required] IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var bytes = new byte[file.Length];
        _ = await stream.ReadAsync(bytes.AsMemory(0, (int)file.Length));
        await settingsService.SetAvatarAsync(HttpContext.User, bytes, file.ContentType);
    }
}
