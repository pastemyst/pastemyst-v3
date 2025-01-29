using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Models;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers;

[ApiController]
[Route("/api/v3/langs")]
public sealed class LanguagesController(LanguageProvider languageProvider) : ControllerBase
{
    [HttpGet]
    public List<Language> All()
    {
        return languageProvider.Languages;
    }

    [HttpGet("popular")]
    public List<string> Popular()
    {
        return languageProvider.PopularLanguageNames;
    }

    [HttpGet("{name}")]
    public Language FindByName(string name)
    {
        return languageProvider.FindByName(name);
    }

    [HttpPost("autodetect")]
    public Task<Language> AutodetectLanguage([FromBody] string content, CancellationToken cancellationToken)
    {
        return languageProvider.AutodetectLanguageAsync(content, cancellationToken);
    }
}
