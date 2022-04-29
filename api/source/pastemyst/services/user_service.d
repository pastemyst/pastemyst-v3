module pastemyst.services.user_service;

import pastemyst.models;
import pastemyst.services;
import pastemyst.utils;
import std.typecons;

@safe:

/**
 * Service for handling user stuff.
 */
public class UserService
{
    private MongoService mongoService;

    ///
    public this(MongoService mongoService)
    {
        this.mongoService = mongoService;
    }

    ///
    public Nullable!User findById(string id)
    {
        return mongoService.findOneById!User(id);
    }

    ///
    public bool existsById(string id)
    {
        return !findById(id).isNull();
    }

    /**
     * Checks if a user exists with the provided OAuth provider id.
     */
    public bool existsByProviderId(const string provider, const string id)
    {
        return !findByProviderId(provider, id).isNull();
    }

    /**
     * Finds a user with the provided OAuth provider id.
     */
    public Nullable!User findByProviderId(const string provider, const string id)
    {
        return mongoService.findOne!User(["oauthProviderIds." ~ provider: id]);
    }

    ///
    public bool existsByUsername(const string username)
    {
        return !findByUsername(username).isNull();
    }

    ///
    public Nullable!User findByUsername(const string username)
    {
        return mongoService.findOne!User(["username": username]);
    }

    ///
    public Nullable!User findById(const string id)
    {
        return mongoService.findOneById!User(id);
    }

    /**
     * Inserts a user into the DB. Modifies the provided user's ID field with the generated one.
     */
    public void createUser(ref User user)
    {
        user.id = randomIdPred(&existsById);

        mongoService.insert!User(user);
    }

    /**
     * Checks if the provided username is valid. Usernames must be unique, can contain alphanumeric chars and these symbols: ., -, _
     *
     * It does check if the username is already taken.
     *
     * Returns: `null` if username is valid, otherwise returns an explanation message.
     */
    public string validateUsername(const string username)
    {
        import std.regex : ctRegex, matchFirst;

        const rgx = ctRegex!(r"^[\w.-]+$");

        if (existsByUsername(username)) return "Username is already taken.";

        if (username.length == 0) return "Username can't be empty.";

        if (matchFirst(username, rgx).empty) return "Username contains invalid symbols.";

        return null;
    }
}
