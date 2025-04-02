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
    public async Task<IActionResult> GetUser(string username, [FromQuery] string id, CancellationToken cancellationToken)
    {
        var user = await userProvider.GetByUsernameOrIdAsync(username, id, cancellationToken);

        return user is not null ? Ok(user) : NotFound();
    }

    [HttpDelete("{username}")]
    public async Task DeleteUser(string username, CancellationToken cancellationToken)
    {
        await userProvider.DeleteUserAsync(username, cancellationToken);
    }

    [HttpGet("{username}/pastes")]
    public async Task<Page<PasteWithLangStats>> GetUserOwnedPastes(string username, [FromQuery] PageRequest pageRequest, [FromQuery] string tag, CancellationToken cancellationToken)
    {
        return await userProvider.GetOwnedPastesAsync(username, tag, false, pageRequest, cancellationToken);
    }

    [HttpGet("{username}/pastes/pinned")]
    public async Task<Page<PasteWithLangStats>> GetUserOwnedPinnedPastes(string username, [FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        return await userProvider.GetOwnedPastesAsync(username, null, true, pageRequest, cancellationToken);
    }

    [HttpGet("{username}/tags")]
    public async Task<List<string>> GetUserTags(string username, CancellationToken cancellationToken)
    {
        return await userProvider.GetTagsAsync(username, cancellationToken);
    }

    [HttpGet("{username}.zip")]
    public async Task<FileContentResult> DownloadUserData(string username, CancellationToken cancellationToken)
    {
        var (zip, filename) = await userProvider.DownloadUserData(username, cancellationToken);

        return File(zip, "application/zip", filename);
    }
}
