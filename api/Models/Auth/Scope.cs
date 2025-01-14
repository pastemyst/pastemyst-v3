using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace pastemyst.Models.Auth;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum Scope
{
    [EnumMember(Value = "paste:create")]
    CreatePaste
}
