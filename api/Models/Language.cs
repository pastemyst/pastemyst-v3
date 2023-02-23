namespace pastemyst.Models;

public class Language
{
    public string? Name { get; set; }
    
    public string? Type { get; set; }
    
    public List<string> Aliases { get; set; } = new();
    
    public string? CodemirrorMode { get; set; }
    
    public string? CodemirrorMimeType { get; set; }
    
    public bool Wrap { get; set; }

    public List<string> Extensions { get; set; } = new();
    
    public string? Color { get; set; }
    
    public string? TmScope { get; set; }
}