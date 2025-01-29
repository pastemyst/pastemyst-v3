using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models;

[BsonKnownTypes(typeof(Paste), typeof(EncryptedPaste))]
public abstract class BasePaste
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; }

    public string Title { get; set; } = "";

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ExpiresIn ExpiresIn { get; init; } = ExpiresIn.Never;

    public DateTime? DeletesAt { get; init; }

    public string OwnerId { get; init; }

    public bool Private { get; init; }

    public bool Pinned { get; init; }

    public List<string> Tags { get; set; } = [];

    [JsonIgnore] public List<string> Stars { get; init; } = [];

    [JsonPropertyName("stars")] public int StarsCount => Stars.Count;
}
