using System.ComponentModel.DataAnnotations;

namespace PasteMyst.Web.Models;

public class PasteCreateInfo
{
    [MaxLength(128)]
    public string Title { get; set; } = "";

    [MinLength(1)]
    [Required]
    public List<PastyCreateInfo> Pasties { get; init; }

    public ExpiresIn ExpiresIn { get; init; } = ExpiresIn.Never;

    public bool Anonymous { get; init; } = false;

    public bool Private { get; init; } = false;

    public bool Pinned { get; init; } = false;

    public bool Encrypted { get; init; } = false;

    public List<string> Tags { get; init; } = [];
}
