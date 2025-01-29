using System.Reflection;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace PasteMyst.Web.Serializers;

public class CustomEnumStringSerializer<TEnum> : EnumSerializer<TEnum>
    where TEnum : struct, System.Enum, IComparable, IFormattable, IConvertible
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TEnum value)
    {
        var enumValue = typeof(TEnum)
            .GetTypeInfo()
            .GetFields()
            .SingleOrDefault(e => e.Name == value.ToString())!
            .GetCustomAttribute<EnumMemberAttribute>(false)
            ?.Value;

        context.Writer.WriteString(enumValue);
    }

    public override TEnum Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var enumValue = context.Reader.ReadString();

        return (TEnum) typeof(TEnum)
            .GetTypeInfo()
            .GetFields()
            .SingleOrDefault(e => e?.GetCustomAttribute<EnumMemberAttribute>()?.Value == enumValue)
            ?.GetValue(null)!;
    }
}
