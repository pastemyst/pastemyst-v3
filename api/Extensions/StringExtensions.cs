namespace pastemyst.Extensions;

public static class StringExtensions
{
    public static bool EqualsIgnoreCase(this string str, string other)
    {
        return string.Equals(str, other, StringComparison.CurrentCultureIgnoreCase);
    }
}