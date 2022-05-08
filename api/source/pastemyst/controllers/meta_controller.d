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
     * GET /api/v3/meta/releases
     *
     * Returns the list of all releases.
     */
    Release[] getReleases() @safe;

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
    private ChangelogService changelogService;

    ///
    public this(PasteService pasteService, ChangelogService changelogService)
    {
        this.pasteService = pasteService;
        this.changelogService = changelogService;
    }

    public override Json getVersion() @safe
    {
        return Json(["version": Json(changelogService.ver)]);
    }

    public override Release[] getReleases() @safe
    {
        return changelogService.releases;
    }

    public override Json getActivePastes() @safe
    {
        return Json(["count": Json(pasteService.getNumActivePastes())]);
    }
}