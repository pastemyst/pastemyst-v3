using System.Reflection;
using System.Runtime.Serialization;
using PasteMyst.Web.Models;

namespace PasteMyst.Web.Utils;

public static class ExpiresInUtils
{
    public static DateTime? ToDeletesAt(DateTime start, ExpiresIn expiresIn)
    {
        return expiresIn switch
        {
            ExpiresIn.Never => null,
            ExpiresIn.OneHour => start.AddHours(1),
            ExpiresIn.TwoHours => start.AddHours(2),
            ExpiresIn.TenHours => start.AddHours(10),
            ExpiresIn.OneDay => start.AddDays(1),
            ExpiresIn.TwoDays => start.AddDays(2),
            ExpiresIn.OneWeek => start.AddDays(7),
            ExpiresIn.OneMonth => start.AddMonths(1),
            ExpiresIn.OneYear => start.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(expiresIn), expiresIn, null)
        };
    }

    public static ExpiresIn? Parse(string expiresIn)
    {
        foreach (var field in typeof(ExpiresIn).GetFields())
        {
            var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
            if (attribute is not null && attribute.Value == expiresIn)
            {
                return (ExpiresIn) field.GetValue(null);
            }
        }

        if (!Enum.TryParse(expiresIn, out ExpiresIn expiresInEnum))
        {
            return null;
        }

        return expiresInEnum;
    }
}