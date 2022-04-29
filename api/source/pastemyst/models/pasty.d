module pastemyst.models.pasty;

import vibe.d;

/**
 * Represents a single file in a paste.
 */
public struct Pasty
{
    ///
    @name("_id")
    public string id;

    ///
    public string title;

    ///
    public string content;
}
