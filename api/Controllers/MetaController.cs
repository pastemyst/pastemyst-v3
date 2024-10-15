using Microsoft.AspNetCore.Mvc;
using pastemyst.Models;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/meta")]
public class MetaController(
    VersionProvider versionProvider,
    ChangelogProvider changelogProvider,
    PasteService pasteService,
    StatsService statsService)
    : ControllerBase
{
    [HttpGet("version")]
    public VersionResponse GetVersion()
    {
        return new VersionResponse { Version = versionProvider.Version };
    }

    [HttpGet("releases")]
    public List<Release> GetReleases()
    {
        return changelogProvider.Releases;
    }

    [HttpGet("active_pastes")]
    public async Task<ActivePastesResponse> GetActivePastesCount()
    {
        var count = await pasteService.GetActiveCountAsync();

        return new ActivePastesResponse
        {
            Count = count
        };
    }

    [HttpGet("stats")]
    public async Task<AppStats> GetStats()
    {
        return await statsService.GetAppStatsAsync();
    }
}
