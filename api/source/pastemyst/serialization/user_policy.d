module pastemyst.serialization.user_policy;

import pastemyst.models;
import vibe.d;

/**
 * A specific serialization policy for the User struct.
 * Used in any API interface that returns the User object to the caller.
 * It hides the oauth service IDs tied to the user from the public.
 *
 * The reason this is needed is because oauth service IDs are used internally
 * and stored in the mongo DB. The issue is that both mongo and API interfaces
 * use the same serialization system, so this is the only way to hide it from
 * just one place.
 */
public template UserPolicy(T) if (is(T : User))
{
    /**
     * Converts a User struct to a JSON string without the oauthProviderIds field.
     */
    public static Json toRepresentation(in T data) @safe
    {
        auto json = serializeToJson(data);
        json.remove(User.oauthProviderIds.stringof);

        return json;
    }

    /**
     * Converts a JSON string to a User struct without any modifications.
     */
    public static T fromRepresentation(in Json json) @safe
    {
        return deserializeJson!T(json);
    }
}
