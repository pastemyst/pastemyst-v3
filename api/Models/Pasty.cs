using System.ComponentModel.DataAnnotations;

namespace pastemyst.Models;

public class Pasty
{
    public string Id { get; set; }

    public string Title { get; set; } = "";
    
    [Required]
    public string Content { get; set; }
    
    public string Language { get; set; }
}