using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models.V2;

public class ApiKeyV2
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public required string Id { get; init; }
    public required string Key { get; set; }
}
