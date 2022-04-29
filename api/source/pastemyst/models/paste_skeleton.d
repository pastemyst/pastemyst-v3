module pastemyst.models.paste_skeleton;

import pastemyst.models.pasty_skeleton;
import vibe.d;

/**
 * Structure used for creating new pastes.
 */
public struct PasteSkeleton
{
    ///
    @optional
    public string title = "";

    ///
    public PastySkeleton[] pasties;
}
