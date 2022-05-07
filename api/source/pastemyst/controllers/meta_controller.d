module pastemyst.controllers.meta_controller;

import pastemyst.services;
import vibe.d;

/**
 * API /api/v3/meta
 *
 * Endpoints about site information.
 */
@path("/api/v3/meta")
public interface IMetaController
{
    /**
     * GET /api/v3/meta/version
     *
     * Returns the current version of the website. Based on git tags.
     */
    Json getVersion() @safe;

    /**
     * GET /api/v3/meta/active_pastes
     *
     * Returns the number of currently active (existing) pastes.
     */
    Json getActivePastes() @safe;
}

/**
 * API /api/v3/meta
 *
 * Endpoints about site information.
 */
public class MetaController : IMetaController
{
    private PasteService pasteService;

    ///
    public this(PasteService pasteService)
    {
        this.pasteService = pasteService;
    }

    public override Json getVersion() @safe
    {
        import std.process : executeShell;
        import std.string : strip;

        // TODO: (#41) maybe cache this? not sure if caching this is gonna even improve the performance
        // currently this runs in less than 6ms on my machine
        const res = executeShell("git describe --tags --abbrev=0");

        return Json(["version": Json(res.output.strip())]);
    }

    public override Json getActivePastes() @safe
    {
        return Json(["count": Json(pasteService.getNumActivePastes())]);
    }
}