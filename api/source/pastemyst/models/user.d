module pastemyst.models.user;

import vibe.d;

///
public struct User
{
    ///
    @name("_id")
    public string id;

    /**
     * Unique username.
     */
    public string username;

    /**
     * List of user IDs for different OAuth providers.
     */
    // TODO: this should be ignored when sending the User object in rest interfaces.
    public string[string] oauthProviderIds;

    /**
     * Link to the user's avatar. Can be either third party (hosted by an OAuth provider)
     * or hosted on pastemyst (when changing the default avatar).
     */
    public string avatarUrl;
}
