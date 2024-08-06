using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace pastemyst.Models;

public class Settings
{
    public string DefaultLanguage { get; set; } = "Text";
    public string DefaultIndentationUnit { get; set; } = "spaces";
    public uint DefaultIndentationWidth { get; set; } = 4;
    public bool TextWrap { get; set; } = true;
    public bool CopyLinkOnCreate { get; set; } = false;
    public string PasteView { get; set; } = "tabbed";
    public string Theme { get; set; } = "myst";
}

public class SessionSettings
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; }

    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;

    public Settings Settings { get; set; } = new Settings();
}
