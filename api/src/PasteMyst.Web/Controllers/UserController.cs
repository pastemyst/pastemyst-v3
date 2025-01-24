using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Models;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers;

[ApiController]
[Route("/api/v3/users")]
public class UserController(UserProvider userProvider) : ControllerBase
{
    [HttpGet]
    [Route("{username}")]
    [Route("")]
    public async Task<IActionResult> GetUser(string username, [FromQuery] string id)
    {
        var user = await userProvider.GetByUsernameOrIdAsync(username, id);

        return user is not null ? Ok(user) : NotFound();
    }

    [HttpDelete("{username}")]
    public async Task DeleteUser(string username)
    {
        await userProvider.DeleteUserAsync(username);
    }

    [HttpGet("{username}/pastes")]
    public async Task<Page<PasteWithLangStats>> GetUserOwnedPastes(string username, [FromQuery] PageRequest pageRequest, [FromQuery] string tag)
    {
        return await userProvider.GetOwnedPastesAsync(username, tag, false, pageRequest);
    }

    [HttpGet("{username}/pastes/pinned")]
    public async Task<Page<PasteWithLangStats>> GetUserOwnedPinnedPastes(string username, [FromQuery] PageRequest pageRequest)
    {
        return await userProvider.GetOwnedPastesAsync(username, null, true, pageRequest);
    }

    [HttpGet("{username}/tags")]
    public async Task<List<string>> GetUserTags(string username)
    {
        return await userProvider.GetTagsAsync(username);
    }
}
