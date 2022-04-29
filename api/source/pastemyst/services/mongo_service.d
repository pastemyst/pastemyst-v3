module pastemyst.services.mongo_service;

import pastemyst.models;
import pastemyst.services;
import std.typecons;
import vibe.d;

@safe:

/**
 * Service for handling MongoDB stuff.
 */
public class MongoService
{
    private MongoDatabase db;

    ///
    public this(ConfigService config)
    {
        db = connectMongoDB(config.mongoConnectionString).getDatabase(config.mongoDatabase);

        IndexOptions idxOpts;
        idxOpts.unique = true;
        db["users"].createIndex(["username": "text"]);
        db["users"].createIndex(["username": 1], idxOpts);
    }

    /**
     * Resolves a type to a mongo collection name.
     */
    private string getCollectionName(T)() const
    {
        static if (is(T == User))
        {
            return "users";
        }
        else static if (is(T == Paste))
        {
            return "pastes";
        }
        else
        {
            static assert(false, "Cannot get a collection name from the type " ~ T.stringof);
        }
    }

    /**
     * Finds one element based on the query.
     */
    public Nullable!R findOne(R, T)(T query)
    {
        auto collection = db[getCollectionName!R()];

        return collection.findOne!R(query);
    }

    /**
     * Finds one element based on the id. Same as `findOne(["_id": id])`
     */
    public Nullable!R findOneById(R, T)(T id)
    {
        return findOne!R(["_id": id]);
    }

    /**
     * Inserts an element into the DB.
     */
    public void insert(T)(T element)
    {
        auto collection = db[getCollectionName!T()];

        collection.insert(element);
    }
}
