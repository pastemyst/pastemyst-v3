module pastemyst.controllers.paste_controller;

import pastemyst.models;
import pastemyst.services;
import std.datetime;
import vibe.d;

/**
 * API /api/v3/paste
 */
@path("/api/v3/paste")
public interface IPasteController
{
    /**
     * GET /api/v3/paste/:id
     *
     * Returns the user from the provided ID.
     */
    @path("/:id")
    const(Paste) getPaste(string _id) @safe;

    /**
     * POST /api/v3/paste/
     *
     * Creates a new paste from the provided paste skeleton.
     */
    @path("/")
    @method(HTTPMethod.POST)
    @bodyParam("skeleton")
    const(Paste) createPaste(PasteSkeleton skeleton) @safe;
}

/**
 * API /api/v3/paste
 */
public class PasteController : IPasteController
{
    private PasteService pasteService;

    ///
    public this(PasteService pasteService)
    {
        this.pasteService = pasteService;
    }

    public override const(Paste) getPaste(string _id) @safe
    {
        const paste = pasteService.findById(_id);

        if (paste.isNull()) throw new HTTPStatusException(HTTPStatus.notFound);

        return paste.get();
    }

    public override const(Paste) createPaste(PasteSkeleton skeleton) @safe
    {
        auto res = Paste();

        res.title = skeleton.title;
        res.createdAt = cast(DateTime) Clock.currTime(UTC());

        foreach (pastySkel; skeleton.pasties)
        {
            auto pasty = Pasty();
            pasty.title = pastySkel.title;
            pasty.content = pastySkel.content;

            res.pasties ~= pasty;
        }

        pasteService.createPaste(res);

        return res;
    }
}
