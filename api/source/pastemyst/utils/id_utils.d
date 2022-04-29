module pastemyst.utils.id_utils;

import pastemyst.utils.base36;

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
