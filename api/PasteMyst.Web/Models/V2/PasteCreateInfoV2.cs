using System.ComponentModel.DataAnnotations;

namespace PasteMyst.Web.Models.V2;

public class PasteCreateInfoV2
{
    [MaxLength(128)]
    public string Title { get; set; } = "";

    [MinLength(1)]
    [Required]
    public List<PastyCreateInfoV2> Pasties { get; init; }

    public ExpiresIn ExpiresIn { get; init; } = ExpiresIn.Never;

    public bool IsPrivate { get; init; } = false;

    public bool IsPublic  { get; init; } = false;

    public string Tags { get; init; } = "";
}