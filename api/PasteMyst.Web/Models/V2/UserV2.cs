using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models.V2;

public class UserV2
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [JsonPropertyName("_id")]
    public string Id { get; init; }

    public string Username { get; set; }

    public Dictionary<string, string> ServiceIds { get; set; }

    public bool Contributor { get; set; }

    public List<string> Stars { get; set; }

    public string AvatarUrl { get; set; }

    public bool PublicProfile { get; set; }

    public string DefaultLang { get; set; }

    public int SupporterLength { get; set; }
}
