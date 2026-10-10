using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PasteMyst.Web.Models.V2;

public class PastyEditInfoV2
{
    // v2 clients send "_id", like the responses use
    [JsonPropertyName("_id")]
    public string Id { get; init; }

    [MaxLength(50)]
    public string Title { get; set; }

    [Required]
    public string Code { get; set; }

    public string Language { get; set; }
}
