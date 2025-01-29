using System.Text.RegularExpressions;
using Octokit;
using Release = PasteMyst.Web.Models.Release;

namespace PasteMyst.Web.Services;

public sealed class ChangelogProvider
{
    private List<Release> _releases;

    public async Task<IEnumerable<Release>> GenerateChangelogAsync()
    {
        // If we've cached the releases before, then just return it
        if (_releases != null)
            return _releases;

        // Otherwise, fetch it and cache it for subsequent requests
        var github = new GitHubClient(new ProductHeaderValue("pastemyst"));

        var v2Releases = await github.Repository.Release.GetAll("codemyst", "pastemyst");
        var v3Releases = await github.Repository.Release.GetAll("pastemyst", "pastemyst-v3");

        // Ignore drafts.
        _releases =
        [
            ..v3Releases.Where(x => !x.Draft).Select(ToRelease),
            ..v2Releases.Where(x => !x.Draft).Select(ToRelease)
        ];

        return _releases;
    }

    private static Release ToRelease(Octokit.Release release)
    {
        // If it doesn't have a title use the tag as the title.
        var title = release.Name;
        if (string.IsNullOrEmpty(title))
        {
            title = release.TagName;
        }

        // Prepend with 'v' if it's missing.
        if (title[0] != 'v')
        {
            title = "v" + title;
        }

        // Remove ## changelog from older releases.
        var content = Regex.Replace(release.Body, "(?i)## changelog:?\r\n\r\n", "");

        return new Release
        {
            Url = release.HtmlUrl,
            Title = title,
            Content = content,
            IsPrerelease = release.Prerelease,
            ReleasedAt = release.PublishedAt
        };
    }
}
