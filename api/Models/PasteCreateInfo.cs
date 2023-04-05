using System.ComponentModel.DataAnnotations;

namespace pastemyst.Models;

public class PasteCreateInfo
{
    [MaxLength(128)]
    public string Title { get; set; }
    
    [MinLength(1)]
    [Required]
    public List<PastyCreateInfo> Pasties { get; set; }

    public ExpiresIn ExpiresIn { get; set; } = ExpiresIn.Never;

    public bool Anonymous { get; set; } = false;

    public bool Private { get; set; } = false;

    public bool Pinned { get; set; } = false;
    
    public List<string> Tags { get; set; } = new();
}