using Microsoft.AspNetCore.Mvc;

namespace pastemyst.Controllers;

[ApiController]
[Route("api/v3/ping")]
public class PingController : ControllerBase
{
    [HttpGet]
    public void Get()
    {
    }
}