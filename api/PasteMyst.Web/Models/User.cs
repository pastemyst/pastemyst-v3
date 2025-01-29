using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Username { get; set; }

    public string AvatarId { get; set; }

    public bool IsContributor { get; set; }

    public bool IsSupporter { get; set; }

    public bool IsAdmin { get; set; }

    [JsonIgnore] public string ProviderName { get; set; }

    [JsonIgnore] public string ProviderId { get; set; }

    [JsonIgnore] public UserSettings UserSettings { get; set; }

    [JsonIgnore] public Settings Settings { get; set; }
}
