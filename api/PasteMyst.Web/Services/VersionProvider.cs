using LibGit2Sharp;
using Semver;

namespace PasteMyst.Web.Services;

public sealed class VersionProvider
{
    private string version = null!;

    public string GetVersion()
    {
        // If we've already calculated the version, return it
        if (version is not null)
            return version;

        // Otherwise, get the version information from the .git folder in the repository
        var repoPath = "../../";
        if (!Repository.IsValid("../../"))
        {
            repoPath = ".";
        }

        using var repo = new Repository(repoPath);

        version = repo.Tags
            .Select(t => SemVersion.Parse(t.FriendlyName))
            .Max(SemVersion.PrecedenceComparer)
            .ToString();

        return version;
    }
}
