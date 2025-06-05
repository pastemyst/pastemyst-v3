using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models;

public class Settings
{
    public string DefaultLanguage { get; set; } = "Autodetect";
    public string DefaultIndentationUnit { get; set; } = "spaces";
    public uint DefaultIndentationWidth { get; set; } = 4;
    public bool TextWrap { get; set; } = true;
    public bool CopyLinkOnCreate { get; set; } = false;
    public string PasteView { get; set; } = "tabbed";
    public string Theme { get; set; } = "myst";
    public string DarkTheme { get; set; } = "myst";
    public bool FollowSystemTheme { get; set; } = false;
}

public class SessionSettings
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; }

    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;

    public Settings Settings { get; set; } = new Settings();
}
