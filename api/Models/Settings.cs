namespace pastemyst.Models;

public class Settings
{
    public string DefaultLanguage { get; set; } = "Text";
    public string DefaultIndentationUnit { get; set; } = "spaces";
    public uint DefaultIndentationWidth { get; set; } = 4;
}
