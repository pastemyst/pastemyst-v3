using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace pastemyst.Models;

public class Pasty
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }

    public string Title { get; set; } = "";
    
    [Required]
    public string Content { get; set; }
    
    public string Language { get; set; }
}
