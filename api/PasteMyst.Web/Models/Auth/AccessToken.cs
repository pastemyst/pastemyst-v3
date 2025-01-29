using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models.Auth;

public class AccessToken
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; }

    public string Description { get; init; } = "";

    public bool Hidden { get; init; } = false;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; init; } = DateTime.UtcNow.AddDays(30);

    public string Token { get; init; }

    public string OwnerId { get; init; }

    public Scope[] Scopes { get; init; }
}
