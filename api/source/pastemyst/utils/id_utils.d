module pastemyst.utils.id_utils;

import pastemyst.utils.base36;

version (unittest) import fluent.asserts;

/**
 * Keeps generating a new random ID as long as the predicate `p` is true.
 */
public string randomIdPred(bool delegate(string) @safe p, uint length = 8) @safe
{
    string id;

    do
    {
        id = randomBase36(length);
    } while (p(id));

    return id;
}

@("randomIdPred")
unittest
{
    int i = 0;
    import std.stdio;
    auto id = randomIdPred((_) {
        if (i < 5)
        {
            i++;
            return true;
        }

        return false;
     }, 4);

    id.length.should.equal(4);
    i.should.equal(5);
}
