using System.Text.RegularExpressions;
using Octokit;
using Models_Release = PasteMyst.Web.Models.Release;
using Release = PasteMyst.Web.Models.Release;

namespace PasteMyst.Web.Services;

public sealed class ChangelogProvider
{
    public async Task<IEnumerable<Models_Release>> GenerateChangelogAsync()
    {
        var github = new GitHubClient(new ProductHeaderValue("pastemyst"));

        var v2Releases = await github.Repository.Release.GetAll("codemyst", "pastemyst");
        var v3Releases = await github.Repository.Release.GetAll("pastemyst", "pastemyst-v3");

        // Ignore drafts.
        return
        [
            ..v3Releases.Where(x => !x.Draft).Select(ToRelease),
            ..v2Releases.Where(x => !x.Draft).Select(ToRelease)
        ];
    }

    private static Models_Release ToRelease(Octokit.Release release)
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

        return new Models_Release
        {
            Url = release.HtmlUrl,
            Title = title,
            Content = content,
            IsPrerelease = release.Prerelease,
            ReleasedAt = release.PublishedAt
        };
    }
}
