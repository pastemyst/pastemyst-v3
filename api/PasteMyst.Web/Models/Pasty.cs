using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models;

public class Pasty
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; }

    public string Title { get; set; } = "";

    [Required]
    public string Content { get; set; }

    public string Language { get; set; }
}
