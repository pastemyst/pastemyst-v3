using LibGit2Sharp;
using Semver;

namespace PasteMyst.Web.Services;

public class VersionProvider : IHostedService
{
    public string Version { get; private set; } = null!;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var repoPath = "../";
        if (!Repository.IsValid("../"))
        {
            repoPath = ".";
        }

        using var repo = new Repository(repoPath);

        Version = repo.Tags
            .Select(t => SemVersion.Parse(t.FriendlyName, SemVersionStyles.Strict))
            .OrderDescending()
            .First()
            .ToString();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
