using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models;

public class ActionLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ActionLogType Type { get; init; }

    public string ObjectId { get; set; }
}
