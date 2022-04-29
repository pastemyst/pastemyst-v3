module pastemyst.services.paste_service;

import pastemyst.models;
import pastemyst.services;
import pastemyst.utils;
import std.typecons;

@safe:

/**
 * Service for handling pastes.
 */
public class PasteService
{
    private MongoService mongoService;

    ///
    public this(MongoService mongoService)
    {
        this.mongoService = mongoService;
    }

    ///
    public Nullable!Paste findById(string id)
    {
        return mongoService.findOneById!Paste(id);
    }

    ///
    public bool existsById(string id)
    {
        return !findById(id).isNull();
    }

    /**
     * Inserts a paste into the DB. Modifies the provided paste's and its pasties' IDs with the generated ones.
     */
    public void createPaste(ref Paste paste)
    {
        import std.algorithm : canFind;

        paste.id = randomIdPred(&existsById);

        foreach (ref pasty; paste.pasties)
        {
            pasty.id = randomIdPred(id => paste.pasties.canFind!(p => p.id == id));
        }

        mongoService.insert(paste);
    }
}
