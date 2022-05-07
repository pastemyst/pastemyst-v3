module pastemyst.controllers.meta_controller;

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
}

/**
 * API /api/v3/meta
 *
 * Endpoints about site information.
 */
public class MetaController : IMetaController
{
    public override Json getVersion() @safe
    {
        import std.process : executeShell;
        import std.string : strip;

        // TODO: maybe cache this? not sure if caching this is gonna even improve the performance
        // currently this runs in less than 6ms on my machine
        const res = executeShell("git describe --tags --abbrev=0");

        return Json(["version": Json(res.output.strip())]);
    }
}