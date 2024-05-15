using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace pastemyst.Models;

public class Paste
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ExpiresIn ExpiresIn { get; set; } = ExpiresIn.Never;

    public DateTime? DeletesAt { get; set; }

    public string Title { get; set; } = "";

    public List<Pasty> Pasties { get; set; }

    public string OwnerId { get; set; }

    public bool Private { get; set; }

    public bool Pinned { get; set; }

    public List<string> Tags { get; set; } = new();

    [JsonIgnore] public List<string> Stars { get; set; } = new();

    [JsonPropertyName("stars")] public int StarsCount => Stars.Count;
}
