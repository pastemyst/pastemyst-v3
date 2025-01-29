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
    StatsService statsService)
    : ControllerBase
{
    [HttpGet("version")]
    public VersionResponse GetVersion()
    {
        return new VersionResponse { Version = versionProvider.GetVersion() };
    }

    [HttpGet("releases")]
    public async Task<List<Release>> GetReleases(CancellationToken cancellationToken)
    {
        return (await changelogProvider.GenerateChangelogAsync(cancellationToken)).ToList();
    }

    [HttpGet("active_pastes")]
    public async Task<ActivePastesResponse> GetActivePastesCount(CancellationToken cancellationToken)
    {
        var count = await pasteService.GetActiveCountAsync(cancellationToken);

        return new ActivePastesResponse
        {
            Count = count
        };
    }

    [HttpGet("stats")]
    public async Task<AppStats> GetStats(CancellationToken cancellationToken)
    {
        return await statsService.GetAppStatsAsync(cancellationToken);
    }
}
