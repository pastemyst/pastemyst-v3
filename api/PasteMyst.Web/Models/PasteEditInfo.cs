using System.ComponentModel.DataAnnotations;

namespace PasteMyst.Web.Models;

public class PasteEditInfo
{
    [MaxLength(128)]
    public string Title { get; set; }

    // Same rules as creating a paste
    [Required]
    [MinLength(1)]
    public List<PastyEditInfo> Pasties { get; set; }
}
