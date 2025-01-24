using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PasteMyst.Web.Models;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers;

[ApiController]
[Route("api/v3/meta")]
public sealed class MetaController(
    VersionProvider versionProvider,
    ChangelogProvider changelogProvider,
    PasteService pasteService,
    StatsService statsService,
    IMemoryCache memoryCache)
    : ControllerBase
{
    [HttpGet("version")]
    public VersionResponse GetVersion()
    {
        return new VersionResponse { Version = versionProvider.Version };
    }

    [HttpGet("releases")]
    public async Task<List<Release>> GetReleasesAsync()
    {
        if (memoryCache.TryGetValue("releases", out List<Release> releases))
            return releases;

        var updatedReleases = (await changelogProvider.GenerateChangelogAsync()).ToList();

        memoryCache.Set("releases", updatedReleases, DateTimeOffset.Now.AddHours(1));

        return updatedReleases;
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
