using LibGit2Sharp;

namespace pastemyst.Services;

public interface IVersionProvider
{
    string Version { get; }
}

public class VersionProvider : IVersionProvider, IHostedService
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

        Version = repo.Tags.OrderBy(t => t.FriendlyName).Last().FriendlyName;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
