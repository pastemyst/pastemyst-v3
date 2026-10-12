using System.Net;
using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Extensions;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Models.V2;
using PasteMyst.Web.Services;
using PasteMyst.Web.Utils;

namespace PasteMyst.Web.Controllers.V2;

[ApiController]
[Route("/api/v2/user")]
public class UserControllerV2(UserProvider userProvider, IConfiguration configuration, UserContext userContext) : ControllerBase
{
    [HttpGet("{username}/exists")]
    public async Task<JsonResult> GetUserExists(string username, CancellationToken cancellationToken)
    {
        var res = await userProvider.ExistsByUsernameAsync(username, cancellationToken);

        if (!res) throw new HttpException(HttpStatusCode.NotFound, "Not Found");

        // v2 answered with the JSON string "", clients read the JSON content type
        return new JsonResult("");
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
            DefaultLang = V2LanguageMapper.ToV2Name(user.Settings.DefaultLanguage),
            SupporterLength = user.IsSupporter ? 1 : 0,
            Contributor = user.IsContributor
        };
    }

    [HttpGet("self")]
    public async Task<UserV2> GetSelf()
    {
        // v2 answered a missing or invalid key with 400
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.BadRequest, "invalid token.");
        }

        if (!userContext.HasScope(Scope.User, Scope.UserRead))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.UserRead.ToEnumString()}.");
        }

        return new UserV2
        {
            Id = userContext.Self.Id,
            Username = userContext.Self.Username,
            ServiceIds = userContext.Self.ProviderName is null
                ? []
                : new Dictionary<string, string> { { userContext.Self.ProviderName, userContext.Self.ProviderId } },
            Contributor = userContext.Self.IsContributor,
            // Stars were part of the user in v2, so user:read is enough for them
            Stars = [..(await userProvider.GetStarredPastesAsync(userContext.Self.Id)).Select(p => p.Id)],
            AvatarUrl = $"{configuration["Host"]}/api/v3/images/{userContext.Self.AvatarId}",
            PublicProfile = userContext.Self.UserSettings.ShowAllPastesOnProfile,
            DefaultLang = V2LanguageMapper.ToV2Name(userContext.Self.Settings.DefaultLanguage),
            SupporterLength = userContext.Self.IsSupporter ? 1 : 0
        };
    }

    [HttpGet("self/pastes")]
    public async Task<List<string>> GetSelfPastes()
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.BadRequest, "invalid token.");
        }

        return [..(await userProvider.GetSelfOwnedPastesAsync()).OrderByDescending(p => p.CreatedAt).Select(p => p.Id)];
    }
}