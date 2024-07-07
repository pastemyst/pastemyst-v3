namespace pastemyst.Models;

public class Settings
{
    public string DefaultLanguage { get; set; } = "Text";
    public string DefaultIndentationUnit { get; set; } = "spaces";
    public uint DefaultIndentationWidth { get; set; } = 4;
    public bool TextWrap { get; set; } = true;
    public bool CopyLinkOnCreate { get; set; } = false;
    public string PasteView { get; set; } = "tabbed";
    public string Theme { get; set; } = "myst";
}
