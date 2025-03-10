using System.Net;
using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Extensions;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Models.V2;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers.V2;

[ApiController]
[Route("/api/v2/user")]
public class UserControllerV2(UserProvider userProvider, IConfiguration configuration, UserContext userContext) : ControllerBase
{
    [HttpGet("{username}/exists")]
    public async Task GetUserExists(string username, CancellationToken cancellationToken)
    {
        var res = await userProvider.ExistsByUsernameAsync(username, cancellationToken);

        if (!res) throw new HttpException(HttpStatusCode.NotFound, "Not Found");
    }

    [HttpGet("{username}")]
    public async Task<MinimalUserV2> GetUser(string username, CancellationToken cancellationToken)
    {
        var user = await userProvider.GetByUsernameAsync(username, cancellationToken) ?? throw new HttpException(HttpStatusCode.NotFound, "Not Found");

        if (!user.UserSettings.ShowAllPastesOnProfile) throw new HttpException(HttpStatusCode.NotFound, "Not Found");

        return new MinimalUserV2
        {
            Id = user.Id,
            Username = user.Username,
            AvatarUrl = $"{configuration["Host"]}/api/v3/images/{user.AvatarId}",
            PublicProfile = user.UserSettings.ShowAllPastesOnProfile,
            DefaultLang = user.Settings.DefaultLanguage,
            SupporterLength = user.IsSupporter ? 1 : 0,
            Contributor = user.IsContributor
        };
    }

    [HttpGet("self")]
    public async Task<UserV2> GetSelf()
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to get self.");
        }

        if (!userContext.HasScope(Scope.User, Scope.UserRead))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.UserRead.ToEnumString()}.");
        }

        return new UserV2
        {
            Id = userContext.Self.Id,
            Username = userContext.Self.Username,
            ServiceIds = new Dictionary<string, string>
            {
                { userContext.Self.ProviderName, userContext.Self.ProviderId }
            },
            Contributor = userContext.Self.IsContributor,
            Stars = [..(await userProvider.GetSelfStarredPastesAsync()).Select(p => p.Id)],
            AvatarUrl = $"{configuration["Host"]}/api/v3/images/{userContext.Self.AvatarId}",
            PublicProfile = userContext.Self.UserSettings.ShowAllPastesOnProfile,
            DefaultLang = userContext.Self.Settings.DefaultLanguage,
            SupporterLength = userContext.Self.IsSupporter ? 1 : 0
        };
    }

    [HttpGet("self/pastes")]
    public async Task<List<string>> GetSelfPastes()
    {
        return [..(await userProvider.GetSelfOwnedPastesAsync()).OrderByDescending(p => p.CreatedAt).Select(p => p.Id)];
    }
}