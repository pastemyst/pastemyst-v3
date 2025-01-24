using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models;

public class PasteHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; }

    public DateTime EditedAt { get; init; } = DateTime.UtcNow;

    public string Title { get; set; }

    public List<Pasty> Pasties { get; set; }
}

public class PasteHistoryCompact
{
    public string Id { get; init; }

    public DateTime EditedAt { get; init; }
}
