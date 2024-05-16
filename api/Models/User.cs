using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace pastemyst.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Username { get; set; }

    public string AvatarId { get; set; }

    public bool IsContributor { get; set; }

    public bool IsSupporter { get; set; }

    public bool isAdmin { get; set; }

    [JsonIgnore] public string ProviderName { get; set; }

    [JsonIgnore] public string ProviderId { get; set; }

    [JsonIgnore] public UserSettings Settings { get; set; }
}
