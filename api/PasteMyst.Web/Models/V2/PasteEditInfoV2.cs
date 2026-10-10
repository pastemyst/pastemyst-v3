using System.ComponentModel.DataAnnotations;

namespace PasteMyst.Web.Models.V2;

/// <summary>
/// Every field is optional, as in v2: null (omitted) means unchanged.
/// </summary>
public class PasteEditInfoV2
{
    [MaxLength(128)]
    public string Title { get; set; }

    [MinLength(1)]
    public List<PastyEditInfoV2> Pasties { get; init; }

    public bool? IsPrivate { get; init; }

    public bool? IsPublic { get; init; }

    public string Tags { get; init; }
}
