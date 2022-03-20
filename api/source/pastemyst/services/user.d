module pastemyst.services.user;

import pastemyst.services;

public class UserService
{
    private MongoService mongoService;

    public this(MongoService mongoService)
    {
        this.mongoService = mongoService;
    }
}