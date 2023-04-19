using Microsoft.AspNetCore.Mvc;
using pastemyst.Models;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/meta")]
public class MetaController : ControllerBase
{
    private readonly IVersionProvider _versionProvider;
    private readonly IChangelogProvider _changelogProvider;
    private readonly IPasteService _pasteService;
    private readonly IStatsService _statsService;

    public MetaController(IVersionProvider versionProvider, IChangelogProvider changelogProvider,
        IPasteService pasteService, IStatsService statsService)
    {
        _versionProvider = versionProvider;
        _changelogProvider = changelogProvider;
        _pasteService = pasteService;
        _statsService = statsService;
    }

    [HttpGet("version")]
    public VersionResponse GetVersion()
    {
        return new VersionResponse { Version = _versionProvider.Version };
    }

    [HttpGet("releases")]
    public List<Release> GetReleases()
    {
        return _changelogProvider.Releases;
    }

    [HttpGet("active_pastes")]
    public async Task<ActivePastesResponse> GetActivePastesCount()
    {
        var count = await _pasteService.GetActiveCountAsync();

        return new ActivePastesResponse
        {
            Count = count
        };
    }

    [HttpGet("stats")]
    public async Task<AppStats> GetStats()
    {
        return await _statsService.GetAppStatsAsync();
    }
}
