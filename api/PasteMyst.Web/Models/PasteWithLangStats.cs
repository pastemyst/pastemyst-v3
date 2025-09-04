namespace PasteMyst.Web.Models;

public class PasteWithLangStats
{
    public BasePaste Paste { get; set; }
    public List<LanguageStat> LanguageStats { get; set; }
    public bool Encrypted { get; set; } = false;
}
