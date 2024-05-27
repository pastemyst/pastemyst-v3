using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace pastemyst.Models;

public class Paste
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ExpiresIn ExpiresIn { get; init; } = ExpiresIn.Never;

    public DateTime? DeletesAt { get; init; }

    public string Title { get; set; } = "";

    public List<Pasty> Pasties { get; set; }

    public string OwnerId { get; init; }

    public bool Private { get; set; }

    public bool Pinned { get; set; }

    public List<string> Tags { get; set; } = new();

    [JsonIgnore] public List<string> Stars { get; set; } = new();

    [JsonPropertyName("stars")] public int StarsCount => Stars.Count;
}
