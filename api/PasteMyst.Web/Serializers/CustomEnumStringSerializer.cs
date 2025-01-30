using System.Reflection;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace PasteMyst.Web.Serializers;

public class CustomEnumStringSerializer<TEnum> : IBsonSerializer<TEnum> where TEnum : struct, Enum
{
    public Type ValueType => typeof(TEnum);

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TEnum value)
    {
        var enumMemberValue = value.GetType()
            .GetField(value.ToString())
            ?.GetCustomAttribute<EnumMemberAttribute>()
            ?.Value ?? value.ToString();

        context.Writer.WriteString(enumMemberValue);
    }

    public TEnum Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var enumString = context.Reader.ReadString();

        foreach (var field in typeof(TEnum).GetFields())
        {
            var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
            if (attribute is not null && attribute.Value == enumString)
            {
                return (TEnum)field.GetValue(null);
            }
        }

        if (Enum.TryParse(enumString, out TEnum result))
        {
            return result;
        }

        throw new BsonSerializationException($"Cannot deserialize '{enumString}' to {typeof(TEnum).Name}");
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        => Serialize(context, args, (TEnum)value);

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => Deserialize(context, args);
}
