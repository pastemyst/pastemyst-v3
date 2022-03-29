module pastemyst.services.mongo;

import std.typecons;
import vibe.d;
import pastemyst.services;
import pastemyst.models;

@safe:

public class MongoService
{
    private MongoDatabase db;

    public this(ConfigService config)
    {
        db = connectMongoDB(config.mongoConnectionString).getDatabase(config.mongoDatabase);

        IndexOptions idxOpts;
        idxOpts.unique = true;
        db["users"].createIndex(["username": "text"]);
        db["users"].createIndex(["username": 1], idxOpts);
    }

    private string getCollectionName(T)() const
    {
        static if (is(T == User))
        {
            return "users";
        }
        else
        {
            static assert(false, "Cannot get a collection name from the type " ~ T.stringof);
        }
    }

    public Nullable!R findOne(R, T)(T query)
    {
        auto collection = db[getCollectionName!R()];

        return collection.findOne!R(query);
    }

    public Nullable!R findOneById(R, T)(T id)
    {
        return findOne!R(["_id": id]);
    }

    public void insert(T)(T element)
    {
        auto collection = db[getCollectionName!T()];

        collection.insert(element);
    }
}