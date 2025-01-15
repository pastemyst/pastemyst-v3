using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace pastemyst.Models.Auth;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum Scope
{
    /// <summary>
    /// Grants full read and write access to all pastes.
    /// </summary>
    [EnumMember(Value = "paste")]
    Paste,
    /// <summary>
    /// Grants read access to all pastes.
    /// </summary>
    [EnumMember(Value = "paste:read")]
    PasteRead,
    /// <summary>
    /// Grants read and write access to user info.
    /// </summary>
    [EnumMember(Value="user")]
    User,
    /// <summary>
    /// Grants read access to user info.
    /// </summary>
    [EnumMember(Value="user:read")]
    UserRead,
}
