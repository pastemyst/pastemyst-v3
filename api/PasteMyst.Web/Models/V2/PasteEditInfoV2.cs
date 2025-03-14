using System.ComponentModel.DataAnnotations;

namespace PasteMyst.Web.Models.V2;

public class PasteEditInfoV2
{
    [MaxLength(128)]
    public string Title { get; set; } = "";

    [MinLength(1)]
    [Required]
    public List<PastyEditInfoV2> Pasties { get; init; }

    public bool IsPrivate { get; init; } = false;

    public bool IsPublic  { get; init; } = false;

    public string Tags { get; init; } = "";
}