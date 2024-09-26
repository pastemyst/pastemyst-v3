using Microsoft.AspNetCore.Mvc;
using pastemyst.Models;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/langs")]
public class LanguagesController(ILanguageProvider languageProvider) : ControllerBase
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
    public Task<Language> AutodetectLanguage([FromBody] string content)
    {
        return languageProvider.AutodetectLanguageAsync(content);
    }
}
