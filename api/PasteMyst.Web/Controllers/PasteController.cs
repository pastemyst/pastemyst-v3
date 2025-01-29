using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Models;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers;

[ApiController]
[Route("/api/v3/pastes")]
public class PasteController(PasteService pasteService) : ControllerBase
{
    [HttpGet("{pasteId}.zip")]
    public async Task<FileContentResult> GetPasteAsZip(string pasteId, CancellationToken token)
    {
        var (zip, title) = await pasteService.GetPasteAsZip(pasteId, token);

        return File(zip, "application/zip", title + ".zip");
    }

    [HttpGet("{pasteId}")]
    public async Task<Paste> GetPaste(string pasteId)
    {
        return await pasteService.GetAsync(pasteId);
    }

    [HttpGet("{pasteId}/stats")]
    public async Task<PasteStats> GetPasteStats(string pasteId)
    {
        return await pasteService.GetStatsAsync(pasteId);
    }

    [HttpGet("{pasteId}/langs")]
    public async Task<List<LanguageStat>> GetPasteLanguageStats(string pasteId)
    {
        return await pasteService.GetLanguageStatsAsync(pasteId);
    }

    [HttpGet("{pasteId}/history_compact")]
    public async Task<List<PasteHistoryCompact>> GetPasteHistoryCompact(string pasteId)
    {
        return await pasteService.GetHistoryCompactAsync(pasteId);
    }

    [HttpGet("{pasteId}/history/{historyId}")]
    public async Task<Paste> GetPasteAtEdit(string pasteId, string historyId)
    {
        return await pasteService.GetAtEditAsync(pasteId, historyId);
    }

    [HttpGet("{pasteId}/history/{historyId}/diff")]
    public async Task<PasteDiff> GetPasteDiff(string pasteId, string historyId)
    {
        return await pasteService.GetDiffAsync(pasteId, historyId);
    }

    [HttpDelete("{pasteId}")]
    public async Task DeletePaste(string pasteId)
    {
        await pasteService.DeleteAsync(pasteId);
    }

    [HttpGet("{pasteId}/star")]
    public async Task<IActionResult> IsPasteStarred(string pasteId)
    {
        return Ok(await pasteService.IsStarredAsync(pasteId));
    }

    [HttpPost("{pasteId}/star")]
    public async Task ToggleStarPaste(string pasteId)
    {
        await pasteService.ToggleStarAsync(pasteId);
    }

    [HttpPost("{pasteId}/pin")]
    public async Task TogglePinPaste(string pasteId)
    {
        await pasteService.TogglePinnedAsync(pasteId);
    }

    [HttpPost("{pasteId}/private")]
    public async Task TogglePrivatePaste(string pasteId)
    {
        await pasteService.TogglePrivateAsync(pasteId);
    }

    [HttpPatch("{pasteId}")]
    public async Task<Paste> EditPaste(string pasteId, [FromBody] PasteEditInfo editInfo, CancellationToken token)
    {
        return await pasteService.EditAsync(pasteId, editInfo, token);
    }

    [HttpPatch("{pasteId}/tags")]
    public async Task<Paste> EditPasteTags(string pasteId, [FromBody] List<string> tags)
    {
        return await pasteService.EditTagsAsync(pasteId, tags);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaste([FromBody] PasteCreateInfo createInfo, CancellationToken token)
    {
        var paste = await pasteService.CreateAsync(createInfo, token);
        return Created("/api/v3/pastes/" + paste.Id, paste);
    }

    [HttpGet("{pasteId}/encrypted")]
    public async Task<IActionResult> IsPasteEncrypted(string pasteId)
    {
        return Ok(await pasteService.IsEncryptedAsync(pasteId));
    }
}
