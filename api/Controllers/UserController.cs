using Microsoft.AspNetCore.Mvc;
using pastemyst.Models;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/users")]
public class UserController : ControllerBase
{
    private readonly IUserProvider _userProvider;

    public UserController(IUserProvider userProvider)
    {
        _userProvider = userProvider;
    }

    [HttpGet]
    [Route("{username}")]
    [Route("")]
    public async Task<IActionResult> GetUser(string username, [FromQuery] string id)
    {
        var user = await _userProvider.GetByUsernameOrIdAsync(username, id);

        return user is not null ? Ok(user) : NotFound();
    }

    [HttpGet("{username}/pastes")]
    public async Task<Page<Paste>> GetUserOwnedPastes(string username, [FromQuery] PageRequest pageRequest)
    {
        return await _userProvider.GetOwnedPastesAsync(username, pageRequest);
    }
}