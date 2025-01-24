using Microsoft.AspNetCore.Mvc;

namespace PasteMyst.Web.Controllers;

[ApiController]
[Route("api/v3/ping")]
public class PingController : ControllerBase
{
    [HttpGet]
    public void Get()
    {
    }
}