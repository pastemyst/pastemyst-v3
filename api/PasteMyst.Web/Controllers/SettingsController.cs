using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Models;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers;

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
    public async Task UpdateSettings([FromBody] Settings settings, CancellationToken cancellationToken)
    {
        await settingsService.UpdateSettingsAsync(HttpContext, settings, cancellationToken);
    }

    [HttpGet("user")]
    public UserSettings GetUserSettings()
    {
        return settingsService.GetUserSettings();
    }

    [HttpPatch("user")]
    public async Task UpdateUserSettings([FromBody] UserSettings userSettings, CancellationToken cancellationToken)
    {
        await settingsService.UpdateUserSettingsAsync(userSettings, cancellationToken);
    }

    [HttpPatch("username")]
    public async Task SetUsername([FromBody] SetUsernameRequest setUsernameRequest, CancellationToken cancellationToken)
    {
        await settingsService.SetUsernameAsync(setUsernameRequest.Username, cancellationToken);
    }

    [HttpPatch("avatar")]
    public async Task SetAvatar([Required] IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var bytes = new byte[file.Length];
        _ = await stream.ReadAsync(bytes.AsMemory(0, (int)file.Length), cancellationToken);
        await settingsService.SetAvatarAsync(bytes, file.ContentType, cancellationToken);
    }
}
