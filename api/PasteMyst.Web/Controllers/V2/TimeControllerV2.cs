using System.Net;
using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.V2;
using PasteMyst.Web.Utils;

namespace PasteMyst.Web.Controllers.V2;

[ApiController]
[Route("/api/v2/time")]
public class TimeControllerV2 : ControllerBase
{
    [HttpGet("expiresInToUnixTime")]
    public ExpiresInToUnixTimeResultV2 ExpiresInToUnixTime([FromQuery] long createdAt, [FromQuery] string expiresIn)
    {
        ExpiresIn? expiresInEnum = ExpiresInUtils.Parse(expiresIn) ?? throw new HttpException(HttpStatusCode.NotFound, "Invalid expiresIn value.");

        if (expiresInEnum == ExpiresIn.Never)
        {
            return new()
            {
                Result = 0
            };
        }

        var deletesAt = ExpiresInUtils.ToDeletesAt(DateTimeOffset.FromUnixTimeSeconds(createdAt).DateTime, expiresInEnum.Value);

        return new()
        {
            Result = (long)deletesAt.Value.Subtract(DateTime.UnixEpoch).TotalSeconds
        };
    }
}