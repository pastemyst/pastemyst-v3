using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models;

public class Announcement
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Title { get; set; }

    public string Content { get; set; }
}