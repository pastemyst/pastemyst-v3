namespace PasteMyst.Web.Models.V2;

public class LanguageV2
{
    public string Name { get; set; }

    public string Mode { get; set; }

    public List<string> Mimes { get; set; }

    public List<string> Ext { get; set; }

    public string Color { get; set; }
}