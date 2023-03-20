using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace pastemyst.Models;

public class User
{
    public string Id { get; set; }

    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "citext")] public string Username { get; set; }

    [JsonIgnore] public Image Avatar { get; set; }

    public string AvatarId { get; set; }

    public bool IsContributor { get; set; }

    public bool IsSupporter { get; set; }

    [JsonIgnore] public string ProviderName { get; set; }

    [JsonIgnore] public string ProviderId { get; set; }
    
    [JsonIgnore] public UserSettings Settings { get; set; }
}