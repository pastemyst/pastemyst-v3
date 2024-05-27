using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using pastemyst.Models;
using pastemyst.Models.Settings;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/settings")]
public class SettingsController(IUserSettingsService settingsService) : ControllerBase
{
    [HttpGet]
    public UserSettings GetUserSettings()
    {
        return settingsService.GetUserSettings();
    }

    [HttpPatch]
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