using LibGit2Sharp;
using Semver;

namespace pastemyst.Services;

public sealed class VersionProvider
{
    private string _version = null!;

    public string GetVersion()
    {
        // If we've already calculated the version, return it
        if (_version is not null)
            return _version;
        
        // Otherwise, get the version information from the .git folder in the repository
        var repoPath = "../";
        if (!Repository.IsValid("../"))
        {
            repoPath = ".";
        }

        using var repo = new Repository(repoPath);

        _version = repo.Tags
            .Select(t => SemVersion.Parse(t.FriendlyName, SemVersionStyles.Strict))
            .OrderDescending()
            .First()
            .ToString();

        return _version;
    }
}