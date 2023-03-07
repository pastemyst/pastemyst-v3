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
        return await _pasteService.GetPasteAsync(pasteId);
    }

    [HttpGet("{pasteId}/stats")]
    public async Task<PasteStats> GetPasteStats(string pasteId)
    {
        return await _pasteService.GetPasteStatsAsync(pasteId);
    }

    [HttpDelete("{pasteId}")]
    public async Task DeletePaste(string pasteId)
    {
        await _pasteService.DeletePasteAsync(pasteId);
    }

    [HttpGet("{pasteId}/star")]
    public async Task<IActionResult> IsPasteStarred(string pasteId)
    {
        return Ok(await _pasteService.IsPasteStarredAsync(pasteId));
    }

    [HttpPost("{pasteId}/star")]
    public async Task ToggleStarPaste(string pasteId)
    {
        await _pasteService.ToggleStarPasteAsync(pasteId);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaste([FromBody] PasteCreateInfo createInfo)
    {
        var paste = await _pasteService.CreatePasteAsync(createInfo);
        return Created("/api/v3/pastes/" + paste.Id, paste);
    }
}