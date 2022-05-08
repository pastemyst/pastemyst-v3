module pastemyst.services.changelog_service;

import std.regex;
import std.stdio;
import vibe.d;

/**
 * Constains information about a single release.
 */
public struct Release
{
    /**
     * URL to the GitHub release.
     */
    public string url;

    /**
     * Title of the release.
     */
    public string title;

    /**
     * Markdown content.
     */
    public string content;

    /**
     * Whether the release is a pre-release (alpha).
     */
    public bool isPrerelease;

    /**
     * At what time the release was published.
     */
    public SysTime releasedAt;
}

/**
 * Data returned from the GitHub API about releases.
 */
private struct GithubRelease
{
    @name("html_url")
    public string url;

    @name("name")
    public string title;

    @name("tag_name")
    public string tag;

    public string body;

    public bool draft;

    public bool prerelease;

    @name("published_at")
    public string publishedAt;
}

/**
 * Service for generating changelogs from GitHub releases and for getting the current website version.
 */
public class ChangelogService
{
    /**
     * List of all releases.
     */
    public Release[] releases;

    /**
     * Current website version. Can't use the name version because it's a keyword :(
     */
    public string ver;

    ///
    public this()
    {
        // generate the changelog only at start
        // since the app is gonna be restarted on newly published versions
        generateChangelog();
        getVersion();
    }

    /**
     * Fetches the current version of the website. Based on git tags.
     */
    private void getVersion() @safe
    {
        import std.process : executeShell;
        import std.string : strip;

        const res = executeShell("git describe --tags --abbrev=0");

        ver = "v" ~ res.output.strip();
    }

    /**
     * Fetches the releases from GitHub and fills the releases field.
     */
    private void generateChangelog() @safe
    {
        requestHTTP("https://api.github.com/repos/codemyst/pastemyst/releases",
        (scope req)
        {
            req.headers.addField("Accept", "application/vnd.github.v3+json");
            req.headers.addField("User-Agent", "pastemyst");
        },
        (scope res)
        {
            if (res.statusCode != HTTPStatus.ok) return;

            const json = parseJsonString(res.bodyReader.readAllUTF8());

            const ghReleases = deserializeJson!(GithubRelease[])(json);

            releases.length = 0;

            foreach (ghrel; ghReleases)
            {
                if (ghrel.draft) continue; // ignore drafts

                string title = ghrel.title;

                if (title == "")
                {
                    // if release doesn't have title, then use the tag
                    // prepend with v if the tag doesn't have v in it
                    if (ghrel.tag[0] != 'v') title = "v";
                    title ~= ghrel.tag;
                }

                releases ~= Release(ghrel.url, title,
                                    ghrel.body, ghrel.prerelease,
                                    SysTime.fromISOExtString(ghrel.publishedAt));
            }
        });
    }
}