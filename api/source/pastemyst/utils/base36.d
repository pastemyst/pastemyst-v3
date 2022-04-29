module pastemyst.utils.base36;

@safe:

private const string base36Chars = "0123456789abcdefghijklmnopqrstuvwxyz";

/**
 * Generates a random base 36 string.
 */
public string randomBase36(uint length = 8)
{
    import std.array : appender;
    import std.random : uniform;

    auto apndr = appender!string();

    for (int i = 0; i < length; i++)
    {
        apndr.put(base36Chars[uniform!"[)"(0, base36Chars.length)]);
    }

    return apndr.data();
}
