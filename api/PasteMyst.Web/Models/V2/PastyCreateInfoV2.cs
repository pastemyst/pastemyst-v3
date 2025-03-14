using System.ComponentModel.DataAnnotations;

namespace PasteMyst.Web.Models.V2;

public class PastyCreateInfoV2
{
    [MaxLength(50)]
    public string Title { get; set; } = "";

    [Required]
    public string Code { get; set; }

    public string Language { get; set; }
}