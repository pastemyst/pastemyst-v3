using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models.V2;

public class EditV2
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; }

    public int EditId { get; set; }

    public int EditType { get; set; }

    public List<string> Metadata { get; set; }

    public string Edit { get; set; }

    public long EditedAt { get; set; }
}
