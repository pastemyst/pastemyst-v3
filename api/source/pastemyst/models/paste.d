module pastemyst.models.paste;

import pastemyst.models.pasty;
import std.datetime;
import vibe.d;

/**
 * Represents a single paste.
 */
public struct Paste
{
    ///
    @name("_id")
    public string id;

    ///
    public string title;

    ///
    public DateTime createdAt;

    ///
    public Pasty[] pasties;
}
