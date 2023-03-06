using System.ComponentModel.DataAnnotations;

namespace pastemyst.Models;

public class PastyCreateInfo
{
    [MaxLength(50)]
    public string Title { get; set; }
    
    [Required]
    public string Content { get; set; }
    
    public string Language { get; set; }
}