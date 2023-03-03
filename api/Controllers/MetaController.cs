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

    public MetaController(IVersionProvider versionProvider, IChangelogProvider changelogProvider)
    {
        _versionProvider = versionProvider;
        _changelogProvider = changelogProvider;
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
}