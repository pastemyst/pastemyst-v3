using Microsoft.AspNetCore.Mvc;

namespace PasteMyst.Web.Controllers.V1;

/// <summary>
/// The v1 API (`GET /api/paste?id=`, `POST /api/paste`) was still served by v2 but isn't supported in v3.
/// Any call gets a clear 410 instead of a bare 404, and is logged so remaining v1 callers can be spotted.
/// </summary>
[ApiController]
[Route("/api/paste")]
public class PasteControllerV1(ILogger<PasteControllerV1> logger) : ControllerBase
{
    [HttpGet, HttpPost, HttpPut, HttpPatch, HttpDelete]
    [Route("{**rest}")]
    public IActionResult Removed()
    {
        logger.LogWarning("v1 API call: {Method} {Path}{Query} (User-Agent: {UserAgent})",
            Request.Method, Request.Path, Request.QueryString, Request.Headers.UserAgent.ToString());

        return StatusCode(StatusCodes.Status410Gone, new
        {
            statusCode = StatusCodes.Status410Gone,
            message = "The v1 API has been removed. Use the v3 API instead: https://docs.paste.myst.rs"
        });
    }
}
