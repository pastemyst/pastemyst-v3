module pastemyst.services.user;

import pastemyst.services;
import pastemyst.models;
import pastemyst.encoding;

@safe:

public class UserService
{
    private MongoService mongoService;

    public this(MongoService mongoService)
    {
        this.mongoService = mongoService;
    }

    /**
     * Checks if a user exists with the provided OAtuh provider id.
     */
    public bool existsByProviderId(const string provider, const string id)
    {
        return !mongoService.findOne!User(["oauthProviderIds." ~ provider: id]).isNull();
    }

    /**
     * Inserts a user into the DB. Modifies the provided user's ID field with the generated one.
     */
    public void createUser(ref User user)
    {
        // make sure there's no accidental overlap
        string id;
        do
        {
            id = randomBase36();
        } while (!mongoService.findOneById!User(id).isNull());

        user.id = id;

        mongoService.insert!User(user);
    }
}