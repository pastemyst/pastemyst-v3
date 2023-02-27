using Microsoft.AspNetCore.Mvc;
using pastemyst.Models;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/meta")]
public class MetaController : ControllerBase
{
    private readonly IVersionProvider _versionProvider;

    public MetaController(IVersionProvider versionProvider)
    {
        _versionProvider = versionProvider;
    }

    [HttpGet]
    [Route("version")]
    public VersionResponse GetVersion()
    {
        return new VersionResponse { Version = _versionProvider.Version };
    }
}