using System.ComponentModel.DataAnnotations;

namespace PasteMyst.Web.Models.V2;

public class PastyEditInfoV2
{
    public string Id { get; init; }

    [MaxLength(50)]
    public string Title { get; set; }

    [Required]
    public string Code { get; set; }

    public string Language { get; set; }
}
