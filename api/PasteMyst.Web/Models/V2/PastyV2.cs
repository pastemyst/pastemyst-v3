using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models.V2;

public class PastyV2
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [JsonPropertyName("_id")]
    public string Id { get; init; }

    public string Title { get; set; }

    public string Language { get; set; }

    public string Code { get; set; }
}
