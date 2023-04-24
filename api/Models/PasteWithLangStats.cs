namespace pastemyst.Models;

public class PasteWithLangStats
{
    public Paste Paste { get; set; }
    public List<LanguageStat> LanguageStats { get; set; }
}
