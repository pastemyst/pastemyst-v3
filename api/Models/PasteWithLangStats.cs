namespace pastemyst.Models;

public class PasteWithLangStats
{
    public BasePaste Paste { get; set; }
    public List<LanguageStat> LanguageStats { get; set; }
}
