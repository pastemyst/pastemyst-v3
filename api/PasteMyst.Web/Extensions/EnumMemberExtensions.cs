using System.Runtime.Serialization;

namespace PasteMyst.Web.Extensions;

public static class EnumMemberExtensions
{
    public static string ToEnumString<T>(this T type) where T : Enum
    {
        var enumType = typeof(T);
        var name = Enum.GetName(enumType, type);
        var enumMemberAttribute = ((EnumMemberAttribute[])enumType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
        return enumMemberAttribute.Value;
    }
}
