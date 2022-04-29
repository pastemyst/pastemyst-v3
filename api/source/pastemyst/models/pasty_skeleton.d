module pastemyst.models.pasty_skeleton;

import vibe.d;

/**
 * Structure used for creating pasties inside a paste.
 */
public struct PastySkeleton
{
    ///
    @optional
    public string title = "";

    ///
    public string content; 
}
