module pastemyst.services.mongo;

import vibe.d;
import pastemyst.services;

@safe:

public class MongoService
{
    private MongoDatabase db;

    public this(ConfigService config)
    {
        db = connectMongoDB(config.mongoConnectionString).getDatabase(config.mongoDatabase);
    }
}