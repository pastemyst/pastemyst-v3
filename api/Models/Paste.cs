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

    public DateTime? EditedAt {
        get {
            if (History.Count == 0) return null;
            return History[^1].EditedAt;
        }
    }

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

    [JsonIgnore] public List<PasteHistory> History { get; set; } = new();
}

public class PasteHistory
{
    public DateTime EditedAt { get; init; } = DateTime.UtcNow;

    public string Title { get; set; }

    public List<Pasty> Pasties { get; set; }
}
