using Microsoft.AspNetCore.Mvc;

namespace pastemyst.Controllers;

[ApiController]
[Route("api/ping")]
public class PingController : ControllerBase
{
    [HttpGet]
    public void Get()
    {
    }
}