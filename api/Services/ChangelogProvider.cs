using System.Text.RegularExpressions;
using Octokit;
using Release = pastemyst.Models.Release;

namespace pastemyst.Services;

public interface IChangelogProvider
{
    public List<Release> Releases { get; }
}

public class ChangelogProvider : IChangelogProvider, IHostedService
{
    public List<Release> Releases { get; } = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var github = new GitHubClient(new ProductHeaderValue("pastemyst"));

        var v2Releases = await github.Repository.Release.GetAll("codemyst", "pastemyst");
        var v3Releases = await github.Repository.Release.GetAll("pastemyst", "pastemyst-v3");

        Releases.AddRange(GenerateChangelog(v3Releases));
        Releases.AddRange(GenerateChangelog(v2Releases));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private IEnumerable<Release> GenerateChangelog(IEnumerable<Octokit.Release> releases)
    {
        var res = new List<Release>();

        foreach (var release in releases)
        {
            // Ignore drafts.
            if (release.Draft) continue;

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

            res.Add(new Release
            {
                Url = release.HtmlUrl,
                Title = title,
                Content = content,
                IsPrerelease = release.Prerelease,
                ReleasedAt = release.PublishedAt
            });
        }

        return res;
    }
}