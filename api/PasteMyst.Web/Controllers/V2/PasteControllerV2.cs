using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.V2;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers.V2;

[ApiController]
[Route("/api/v2/paste")]
public class PasteControllerV2(PasteService pasteService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<PasteV2> GetPaste(string id)
    {
        var paste = await pasteService.GetAsync(id);

        var pasties = paste.Pasties.Select(pasty => new PastyV2
        {
            Id = pasty.Id,
            Title = pasty.Title,
            Language = pasty.Language,
            Code = pasty.Content
        }).ToList();

        return new()
        {
            Id = paste.Id,
            OwnerId = paste.OwnerId ?? "",
            Title = paste.Title,
            CreatedAt = (long)paste.CreatedAt.Subtract(DateTime.UnixEpoch).TotalSeconds,
            ExpiresIn = paste.ExpiresIn,
            DeletesAt = (long?)paste.DeletesAt?.Subtract(DateTime.UnixEpoch).TotalSeconds ?? 0L,
            Stars = paste.StarsCount,
            IsPrivate = paste.Private,
            IsPublic = paste.Pinned,
            Tags = paste.Tags,
            Pasties = pasties,
            Edits = []
        };
    }

    [HttpPost]
    public async Task<PasteV2> CreatePaste([FromBody] PasteCreateInfoV2 createInfoV2, CancellationToken cancellationToken)
    {
        var createInfo = new PasteCreateInfo
        {
            Title = createInfoV2.Title,
            ExpiresIn = createInfoV2.ExpiresIn,
            Private = createInfoV2.IsPrivate,
            Pinned = createInfoV2.IsPublic,
            Tags = [..createInfoV2.Tags.Split(",").Select(tag => tag.Trim()).Where(tag => tag.Length > 0)],
            Pasties = [..createInfoV2.Pasties.Select(pasty => new PastyCreateInfo
            {
                Title = pasty.Title,
                Language = pasty.Language,
                Content = pasty.Code
            })]
        };

        var paste = await pasteService.CreateAsync(createInfo, cancellationToken);

        var pasties = paste.Pasties.Select(pasty => new PastyV2
        {
            Id = pasty.Id,
            Title = pasty.Title,
            Language = pasty.Language,
            Code = pasty.Content
        }).ToList();

        return new()
        {
            Id = paste.Id,
            OwnerId = paste.OwnerId ?? "",
            Title = paste.Title,
            CreatedAt = (long)paste.CreatedAt.Subtract(DateTime.UnixEpoch).TotalSeconds,
            ExpiresIn = paste.ExpiresIn,
            DeletesAt = (long?)paste.DeletesAt?.Subtract(DateTime.UnixEpoch).TotalSeconds ?? 0L,
            Stars = paste.StarsCount,
            IsPrivate = paste.Private,
            IsPublic = paste.Pinned,
            Tags = paste.Tags,
            Pasties = pasties,
            Edits = []
        };
    }

    [HttpPatch("{id}")]
    public async Task<PasteV2> EditPaste(string id, [FromBody] PasteEditInfoV2 editInfoV2, CancellationToken cancellationToken)
    {
        var paste = await pasteService.GetAsync(id);

        if (editInfoV2.IsPrivate != paste.Private)
        {
            await pasteService.TogglePrivateAsync(id);
        }

        if (editInfoV2.IsPublic != paste.Pinned)
        {
            await pasteService.TogglePinnedAsync(id);
        }

        var tags = editInfoV2.Tags.Split(",").Select(tag => tag.Trim()).Where(t => t.Length > 0).ToList();

        if (tags != paste.Tags)
        {
            await pasteService.EditTagsAsync(id, tags);
        }

        var editInfo = new PasteEditInfo
        {
            Title = editInfoV2.Title,
            Pasties = [..editInfoV2.Pasties.Select(pasty => new PastyEditInfo
            {
                Id = pasty.Id,
                Title = pasty.Title,
                Language = pasty.Language,
                Content = pasty.Code
            })]
        };

        var editedPaste = await pasteService.EditAsync(id, editInfo, cancellationToken);

        var pasties = editedPaste.Pasties.Select(pasty => new PastyV2
        {
            Id = pasty.Id,
            Title = pasty.Title,
            Language = pasty.Language,
            Code = pasty.Content
        }).ToList();

        return new()
        {
            Id = editedPaste.Id,
            OwnerId = editedPaste.OwnerId ?? "",
            Title = editedPaste.Title,
            CreatedAt = (long)editedPaste.CreatedAt.Subtract(DateTime.UnixEpoch).TotalSeconds,
            ExpiresIn = editedPaste.ExpiresIn,
            DeletesAt = (long?)editedPaste.DeletesAt?.Subtract(DateTime.UnixEpoch).TotalSeconds ?? 0L,
            Stars = editedPaste.StarsCount,
            IsPrivate = editedPaste.Private,
            IsPublic = editedPaste.Pinned,
            Tags = editedPaste.Tags,
            Pasties = pasties,
            Edits = []
        };
    }

    [HttpDelete("{id}")]
    public async Task DeletePaste(string id)
    {
        await pasteService.DeleteAsync(id);
    }
}