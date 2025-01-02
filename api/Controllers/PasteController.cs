using Microsoft.AspNetCore.Mvc;
using pastemyst.Models;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/pastes")]
public class PasteController(PasteService pasteService) : ControllerBase
{
    [HttpGet("{pasteId}.zip")]
    public async Task<FileContentResult> GetPasteAsZip(string pasteId)
    {
        var (zip, title) = await pasteService.GetPasteAsZip(HttpContext.User, pasteId);

        return File(zip, "application/zip", title + ".zip");
    }

    [HttpGet("{pasteId}")]
    public async Task<Paste> GetPaste(string pasteId)
    {
        return await pasteService.GetAsync(HttpContext.User, pasteId);
    }

    [HttpGet("{pasteId}/stats")]
    public async Task<PasteStats> GetPasteStats(string pasteId)
    {
        return await pasteService.GetStatsAsync(HttpContext.User, pasteId);
    }

    [HttpGet("{pasteId}/langs")]
    public async Task<List<LanguageStat>> GetPasteLanguageStats(string pasteId)
    {
        return await pasteService.GetLanguageStatsAsync(HttpContext.User, pasteId);
    }

    [HttpGet("{pasteId}/history_compact")]
    public async Task<List<PasteHistoryCompact>> GetPasteHistoryCompact(string pasteId)
    {
        return await pasteService.GetHistoryCompactAsync(HttpContext.User, pasteId);
    }

    [HttpGet("{pasteId}/history/{historyId}")]
    public async Task<Paste> GetPasteAtEdit(string pasteId, string historyId)
    {
        return await pasteService.GetAtEditAsync(HttpContext.User, pasteId, historyId);
    }

    [HttpGet("{pasteId}/history/{historyId}/diff")]
    public async Task<PasteDiff> GetPasteDiff(string pasteId, string historyId)
    {
        return await pasteService.GetDiffAsync(HttpContext.User, pasteId, historyId);
    }

    [HttpDelete("{pasteId}")]
    public async Task DeletePaste(string pasteId)
    {
        await pasteService.DeleteAsync(HttpContext.User, pasteId);
    }

    [HttpGet("{pasteId}/star")]
    public async Task<IActionResult> IsPasteStarred(string pasteId)
    {
        return Ok(await pasteService.IsStarredAsync(HttpContext.User, pasteId));
    }

    [HttpPost("{pasteId}/star")]
    public async Task ToggleStarPaste(string pasteId)
    {
        await pasteService.ToggleStarAsync(HttpContext.User, pasteId);
    }

    [HttpPost("{pasteId}/pin")]
    public async Task TogglePinPaste(string pasteId)
    {
        await pasteService.TogglePinnedAsync(HttpContext.User, pasteId);
    }

    [HttpPost("{pasteId}/private")]
    public async Task TogglePrivatePaste(string pasteId)
    {
        await pasteService.TogglePrivateAsync(HttpContext.User, pasteId);
    }

    [HttpPatch("{pasteId}")]
    public async Task<Paste> EditPaste(string pasteId, [FromBody] PasteEditInfo editInfo)
    {
        return await pasteService.EditAsync(HttpContext.User, pasteId, editInfo);
    }

    [HttpPatch("{pasteId}/tags")]
    public async Task<Paste> EditPasteTags(string pasteId, [FromBody] List<string> tags)
    {
        return await pasteService.EditTagsAsync(HttpContext.User, pasteId, tags);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaste([FromBody] PasteCreateInfo createInfo)
    {
        var paste = await pasteService.CreateAsync(HttpContext.User, createInfo);
        return Created("/api/v3/pastes/" + paste.Id, paste);
    }
}
