using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Models.V2;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers.V2;

[ApiController]
[Route("/api/v2/data")]
public class DataControllerV2(LanguageProvider languageProvider, PasteService pasteService) : ControllerBase
{
    [HttpGet("language")]
    public LanguageV2 GetLanguageByName([FromQuery] string name)
    {
        var lang = languageProvider.FindByName(name);

        return new LanguageV2
        {
            Color = lang.Color,
            Name = lang.Name,
            Ext = [..lang.Extensions.Select(ext => ext[1..])],
            Mimes = [lang.CodemirrorMimeType],
            Mode = lang.CodemirrorMode
        };
    }

    [HttpGet("languageExt")]
    public LanguageV2 GetLanguageByExtension([FromQuery] string extension)
    {
        var lang = languageProvider.FindByName(extension);

        return new LanguageV2
        {
            Color = lang.Color,
            Name = lang.Name,
            Ext = [..lang.Extensions.Select(ext => ext[1..])],
            Mimes = [lang.CodemirrorMimeType],
            Mode = lang.CodemirrorMode
        };
    }

    [HttpGet("numPastes")]
    public async Task<ActivePsatesResponseV2> GetNumPastes(CancellationToken cancellationToken)
    {
        var count = await pasteService.GetActiveCountAsync(cancellationToken);

        return new()
        {
            NumPastes = count
        };
    }
}