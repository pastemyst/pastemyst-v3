using Microsoft.AspNetCore.Mvc;
using pastemyst.Models;
using pastemyst.Services;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/langs")]
public class LanguagesController : ControllerBase
{
    private readonly ILanguageProvider _languageProvider;

    public LanguagesController(ILanguageProvider languageProvider)
    {
        _languageProvider = languageProvider;
    }

    [HttpGet]
    public List<Language> All()
    {
        return _languageProvider.Languages;
    }

    [HttpGet("{name}")]
    public Language FindByName(string name)
    {
        return _languageProvider.FindByName(name);
    }
}
