module pastemyst.utils.base36;

version (unittest) import fluent.asserts;

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

@("randomBase36")
unittest
{
    auto a = randomBase36(8);

    a.length.should.equal(8);

    a = randomBase36(1);

    a.length.should.equal(1);

    a = randomBase36(200);

    a.length.should.equal(200);

    a.should.not.contain(["=", "+", "A", "B", "C", "D", ".", " "]);
}
