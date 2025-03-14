using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models.V2;

[BsonKnownTypes(typeof(PasteV2), typeof(EncryptedPasteV2))]
public class BasePasteV2
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [JsonPropertyName("_id")]
    public string Id { get; init; }

    public string OwnerId { get; set; }

    public long CreatedAt { get; set; }

    public ExpiresIn ExpiresIn { get; set; }

    public long DeletesAt { get; set; }

    public bool IsPrivate { get; set; }

    public bool IsPublic { get; set; }

    public List<string> Tags { get; set; }

    public int Stars { get; set; }

    public bool Encrypted { get; set; }
}
