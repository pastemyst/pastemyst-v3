using System.Text.Json.Serialization;

namespace PasteMyst.Web.Models.V2;

public class MinimalUserV2
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }

    public string Username { get; set; }

    public string AvatarUrl { get; set; }

    public bool PublicProfile { get; set; }

    public string DefaultLang { get; set; }

    public int SupporterLength { get; set; }

    public bool Contributor { get; set; }
}