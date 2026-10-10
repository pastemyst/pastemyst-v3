using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Extensions;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.V2;
using PasteMyst.Web.Services;
using PasteMyst.Web.Utils;

namespace PasteMyst.Web.Controllers.V2;

/// <summary>
/// v2 paste API on top of the v3 paste service. Status codes and messages follow v2 where they
/// differed (v2 answered every edit/delete permission problem with 404, for example).
/// </summary>
[ApiController]
[Route("/api/v2/paste")]
public class PasteControllerV2(PasteService pasteService, UserContext userContext) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<PasteV2> GetPaste(string id)
    {
        return ToV2(await pasteService.GetAsync(id));
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitingExtensions.PasteCreatePolicy)]
    public async Task<PasteV2> CreatePaste([FromBody] PasteCreateInfoV2 createInfoV2, CancellationToken cancellationToken)
    {
        if ((createInfoV2.IsPrivate || createInfoV2.IsPublic || createInfoV2.Tags != "") && !userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Forbidden,
                "can't create a paste using account features without providing a valid Authorization header.");
        }

        if (createInfoV2.IsPrivate && createInfoV2.IsPublic)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "the paste can't be private and shown on the profile");
        }

        var createInfo = new PasteCreateInfo
        {
            Title = createInfoV2.Title,
            ExpiresIn = createInfoV2.ExpiresIn,
            Private = createInfoV2.IsPrivate,
            Pinned = createInfoV2.IsPublic,
            Tags = ParseTags(createInfoV2.Tags),
            Pasties = [..createInfoV2.Pasties.Select(pasty => new PastyCreateInfo
            {
                Title = pasty.Title,
                Language = MapLanguage(pasty.Language),
                Content = pasty.Code
            })]
        };

        try
        {
            return ToV2(await pasteService.CreateAsync(createInfo, cancellationToken));
        }
        catch (LanguageNotFoundException)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "invalid language value.");
        }
    }

    /// <summary>
    /// Every field is optional, as in v2: omitted fields stay unchanged.
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<PasteV2> EditPaste(string id, [FromBody] PasteEditInfoV2 editInfoV2, CancellationToken cancellationToken)
    {
        if (!userContext.IsLoggedIn()) throw new HttpException(HttpStatusCode.NotFound, "Not Found");

        var paste = await pasteService.GetAsync(id);

        if (paste.OwnerId is null || paste.OwnerId != userContext.Self.Id)
        {
            throw new HttpException(HttpStatusCode.NotFound, "Not Found");
        }

        var isPrivate = editInfoV2.IsPrivate ?? paste.Private;
        var isPublic = editInfoV2.IsPublic ?? paste.Pinned;

        if (isPrivate && isPublic)
        {
            throw new HttpException(HttpStatusCode.BadRequest,
                "paste can't be private and shown on the profile at the same time");
        }

        if (editInfoV2.Title is not null || editInfoV2.Pasties is not null)
        {
            var editInfo = new PasteEditInfo
            {
                Title = editInfoV2.Title ?? paste.Title,
                Pasties = editInfoV2.Pasties is null
                    ? [..paste.Pasties.Select(pasty => new PastyEditInfo
                    {
                        Id = pasty.Id,
                        Title = pasty.Title,
                        Language = pasty.Language,
                        Content = pasty.Content
                    })]
                    : [..editInfoV2.Pasties.Select(pasty => new PastyEditInfo
                    {
                        // v2 adds a pasty by sending it with an empty id
                        Id = string.IsNullOrEmpty(pasty.Id) ? null : pasty.Id,
                        Title = pasty.Title,
                        Language = MapLanguage(pasty.Language),
                        Content = pasty.Code
                    })]
            };

            try
            {
                await pasteService.EditAsync(id, editInfo, cancellationToken);
            }
            catch (LanguageNotFoundException)
            {
                throw new HttpException(HttpStatusCode.BadRequest, "invalid language value.");
            }
        }

        if (editInfoV2.Tags is not null)
        {
            var tags = ParseTags(editInfoV2.Tags);
            if (!tags.SequenceEqual(paste.Tags)) await pasteService.EditTagsAsync(id, tags);
        }

        // Private pastes can't be pinned: unpin first, then change privacy, then pin.
        if (!isPublic && paste.Pinned) await pasteService.TogglePinnedAsync(id);
        if (isPrivate != paste.Private) await pasteService.TogglePrivateAsync(id);
        if (isPublic && !paste.Pinned) await pasteService.TogglePinnedAsync(id);

        return ToV2(await pasteService.GetAsync(id));
    }

    [HttpDelete("{id}")]
    public async Task<ContentResult> DeletePaste(string id)
    {
        if (!userContext.IsLoggedIn()) throw new HttpException(HttpStatusCode.NotFound, "Not Found");

        try
        {
            await pasteService.DeleteAsync(id);
        }
        catch (HttpException e) when (e.Status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new HttpException(HttpStatusCode.NotFound, "Not Found");
        }

        // What v2 (vibe.d) sent for a void method: 200, empty body, application/octet-stream.
        // Some clients read the Content-Type of every response and fail without one.
        return new ContentResult { StatusCode = StatusCodes.Status200OK, ContentType = "application/octet-stream", Content = "" };
    }

    private static List<string> ParseTags(string tags) =>
        [..tags.Split(",").Select(tag => tag.Trim()).Where(tag => tag.Length > 0)];

    /// <summary>
    /// v2 language names to v3 ones, the same mapping the migrator uses. Autodetect stays
    /// Autodetect here (the mapper turns it into Text, which is right for migrated pasties only).
    /// </summary>
    private static string MapLanguage(string language)
    {
        if (language is null) return null;
        if (language.Equals("autodetect", StringComparison.OrdinalIgnoreCase)) return "Autodetect";
        return V2LanguageMapper.MapLanguage(language);
    }

    private static PasteV2 ToV2(Paste paste) => new()
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
        Pasties = [..paste.Pasties.Select(pasty => new PastyV2
        {
            Id = pasty.Id,
            Title = pasty.Title,
            Language = V2LanguageMapper.ToV2Name(pasty.Language),
            Code = pasty.Content
        })],
        Edits = []
    };
}
