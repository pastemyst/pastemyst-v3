using System.Text.Json.Serialization;

namespace pastemyst.Models;

public class UserSettings
{
    [JsonIgnore] public User User { get; set; }
    
    public string UserId { get; set; }
    
    public bool ShowAllPastesOnProfile { get; set; } = true;
}