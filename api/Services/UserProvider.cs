using System.Net;
using MongoDB.Driver;
using pastemyst.Exceptions;
using pastemyst.Extensions;
using pastemyst.Models;
using pastemyst.Models.Auth;

namespace pastemyst.Services;

public class UserProvider(UserContext userContext, PasteService pasteService, MongoService mongo, ActionLogger actionLogger, ImageService imageService, AuthService authService)
{
    public async Task<User> GetByUsernameOrIdAsync(string username, string id)
    {
        if (username is not null)
        {
            return await GetByUsernameAsync(username);
        }

        if (id is not null)
        {
            return await mongo.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        return null;
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Username, username);
        return await mongo.Users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await GetByUsernameAsync(username) is not null;
    }

    public async Task<Page<PasteWithLangStats>> GetOwnedPastesAsync(string username, string tag, bool pinnedOnly, PageRequest pageRequest)
    {
        var user = await GetByUsernameAsync(username) ?? throw new HttpException(HttpStatusCode.NotFound, "User not found.");

        // If not showing only pinned pastes, and show all pastes is disabled, return an empty list.
        if (!pinnedOnly && !userContext.UserIsSelf(user) && !user.UserSettings.ShowAllPastesOnProfile)
        {
            return new Page<PasteWithLangStats>();
        }

        var filter = Builders<Paste>.Filter.Eq(p => p.OwnerId, user.Id);

        if (!userContext.UserIsSelf(user) || !userContext.HasScope(Scope.Paste, Scope.PasteRead))
        {
            filter &= Builders<Paste>.Filter.Eq(p => p.Private, false);
        }

        if (pinnedOnly) filter &= Builders<Paste>.Filter.Eq(p => p.Pinned, true);

        if (tag is not null)
        {
            if (!userContext.UserIsSelf(user))
            {
                throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to view paste tags.");
            }

            if (!userContext.HasScope(Scope.User, Scope.UserRead))
            {
                throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.UserRead.ToEnumString()} to view user tags.");
            }

            filter &= Builders<Paste>.Filter.AnyStringIn(p => p.Tags, tag);
        }

        var pastes = await mongo.Pastes.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip(pageRequest.Page * pageRequest.PageSize)
            .Limit(pageRequest.PageSize)
            .ToListAsync();

        var totalItems = await mongo.Pastes.CountDocumentsAsync(filter);
        var totalPages = (int)Math.Ceiling((float)totalItems / pageRequest.PageSize);

        // hide all tags for everyone except the owner
        if (!userContext.UserIsSelf(user))
        {
            pastes.ForEach(p => p.Tags = []);
        }

        var pastesWithLangStags = new List<PasteWithLangStats>();
        foreach (var paste in pastes)
        {
            var stats = pasteService.GetLanguageStats(paste);

            pastesWithLangStags.Add(new() { Paste = paste, LanguageStats = stats });
        }

        return new Page<PasteWithLangStats>
        {
            Items = pastesWithLangStags,
            CurrentPage = pageRequest.Page,
            PageSize = pageRequest.PageSize,
            HasNextPage = pageRequest.Page < totalPages - 1,
            TotalPages = totalPages
        };
    }

    public async Task<List<string>> GetTagsAsync(string username)
    {
        if (!userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to get your own tags.");

        var user = await GetByUsernameAsync(username);

        if (!userContext.UserIsSelf(user))
            throw new HttpException(HttpStatusCode.Unauthorized, "You can only fetch your own tags.");

        if (!userContext.HasScope(Scope.User, Scope.UserRead))
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.UserRead.ToEnumString()}.");

        var filter = Builders<Paste>.Filter.Eq(p => p.OwnerId, user.Id);

        return (await mongo.Pastes.Find(filter).Project(p => p.Tags).ToListAsync())
            .SelectMany(t => t)
            .Distinct()
            .ToList();
    }

    public async Task DeleteUserAsync(string username)
    {
        if (!userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to delete your account.");

        if (!userContext.HasScope(Scope.User))
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.User.ToEnumString()}.");

        var user = await GetByUsernameAsync(username);

        if (userContext.UserIsSelf(user))
            throw new HttpException(HttpStatusCode.Unauthorized, "You can delete only your account.");

        await imageService.DeleteAsync(user.AvatarId);

        await mongo.Pastes.DeleteManyAsync(p => p.OwnerId == user.Id);
        await mongo.Users.DeleteOneAsync(u => u.Id == user.Id);

        // Delete all stars of this user
        var starsFilter = Builders<Paste>.Filter.AnyEq(p => p.Stars, user.Id);
        var starsUpdate = Builders<Paste>.Update.Pull(p => p.Stars, user.Id);
        await mongo.Pastes.UpdateManyAsync(starsFilter, starsUpdate);

        await actionLogger.LogActionAsync(ActionLogType.UserDeleted, user.Id);
    }

    public async Task<GenerateAccessTokenResponse> GenerateAccessTokenForSelf(Scope[] scopes, ExpiresIn expiresIn, string description)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Forbidden, "You must be authorized to generate new access tokens.");
        }

        if (!userContext.HasScope(Scope.UserAccessTokens))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.UserAccessTokens.ToEnumString()}.");
        }

        var (accessToken, expiresAt) = await authService.GenerateAccessToken(userContext.Self, scopes, expiresIn, hidden: false, description);

        return new() { AccessToken = accessToken, ExpiresAt = expiresAt };
    }

    public async Task<List<AccessTokenResponse>> GetAccessTokensForSelf()
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Forbidden, "You must be authorized to generate new access tokens.");
        }

        if (!userContext.HasScope(Scope.UserAccessTokens))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.UserAccessTokens.ToEnumString()}.");
        }

        var filter = Builders<AccessToken>.Filter.Eq(a => a.Hidden, false) & Builders<AccessToken>.Filter.Eq(a => a.OwnerId, userContext.Self.Id);
        var accessTokens = (await mongo.AccessTokens.FindAsync(filter)).ToList().Select(a => new AccessTokenResponse
                {
                    Description = a.Description,
                    CreatedAt = a.CreatedAt,
                    ExpiresAt = a.ExpiresAt,
                    Scopes = a.Scopes
                });

        return accessTokens.ToList();
    }
}
