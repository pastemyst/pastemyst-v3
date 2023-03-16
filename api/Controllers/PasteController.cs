using Microsoft.AspNetCore.Mvc;
using pastemyst.Models;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/pastes")]
public class PasteController : ControllerBase
{
    private readonly IPasteService _pasteService;

    public PasteController(IPasteService pasteService)
    {
        _pasteService = pasteService;
    }

    [HttpGet("{pasteId}")]
    public async Task<Paste> GetPaste(string pasteId)
    {
        return await _pasteService.GetAsync(pasteId);
    }

    [HttpGet("{pasteId}/stats")]
    public async Task<PasteStats> GetPasteStats(string pasteId)
    {
        return await _pasteService.GetStatsAsync(pasteId);
    }

    [HttpGet("{pasteId}/langs")]
    public async Task<List<LanguageStat>> GetPasteLanguageStats(string pasteId)
    {
        return await _pasteService.GetLanguageStatsAsync(pasteId);
    }

    [HttpDelete("{pasteId}")]
    public async Task DeletePaste(string pasteId)
    {
        await _pasteService.DeleteAsync(pasteId);
    }

    [HttpGet("{pasteId}/star")]
    public async Task<IActionResult> IsPasteStarred(string pasteId)
    {
        return Ok(await _pasteService.IsStarredAsync(pasteId));
    }

    [HttpPost("{pasteId}/star")]
    public async Task ToggleStarPaste(string pasteId)
    {
        await _pasteService.ToggleStarAsync(pasteId);
    }

    [HttpPost("{pasteId}/pin")]
    public async Task TogglePinPaste(string pasteId)
    {
        await _pasteService.TogglePinnedAsync(pasteId);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaste([FromBody] PasteCreateInfo createInfo)
    {
        var paste = await _pasteService.CreateAsync(createInfo);
        return Created("/api/v3/pastes/" + paste.Id, paste);
    }
}