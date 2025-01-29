using System.Net;
using MongoDB.Driver;
using PasteMyst.Web.Extensions;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;

namespace PasteMyst.Web.Services;

public class UserProvider(UserContext userContext, PasteService pasteService, MongoService mongo, ActionLogger actionLogger, ImageService imageService)
{
    public async Task<User> GetByUsernameOrIdAsync(string username, string id, CancellationToken cancellationToken)
    {
        if (username is not null)
        {
            return await GetByUsernameAsync(username, cancellationToken);
        }

        if (id is not null)
        {
            return await mongo.Users.Find(u => u.Id == id).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }

        return null;
    }

    public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Username, username);
        return await mongo.Users.Find(filter).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await GetByUsernameAsync(username, cancellationToken) is not null;
    }

    public async Task<Page<PasteWithLangStats>> GetOwnedPastesAsync(string username, string tag, bool pinnedOnly, PageRequest pageRequest, CancellationToken cancellationToken)
    {
        var user = await GetByUsernameAsync(username, cancellationToken) ?? throw new HttpException(HttpStatusCode.NotFound, "User not found.");

        // If not showing only pinned pastes, and show all pastes is disabled, return an empty list.
        if (!pinnedOnly && !userContext.UserIsSelf(user) && !user.UserSettings.ShowAllPastesOnProfile)
        {
            return new Page<PasteWithLangStats>();
        }

        var filter = Builders<BasePaste>.Filter.Eq(p => p.OwnerId, user.Id);

        if (!userContext.UserIsSelf(user) || !userContext.HasScope(Scope.Paste, Scope.PasteRead))
        {
            filter &= Builders<BasePaste>.Filter.Eq(p => p.Private, false);
        }

        if (pinnedOnly) filter &= Builders<BasePaste>.Filter.Eq(p => p.Pinned, true);

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

            filter &= Builders<BasePaste>.Filter.AnyStringIn(p => p.Tags, tag);
        }

        var pastes = await mongo.BasePastes.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip(pageRequest.Page * pageRequest.PageSize)
            .Limit(pageRequest.PageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        var totalItems = await mongo.BasePastes.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var totalPages = (int)Math.Ceiling((float)totalItems / pageRequest.PageSize);

        // hide all tags for everyone except the owner
        if (!userContext.UserIsSelf(user))
        {
            pastes.ForEach(p => p.Tags = []);
        }

        var pastesWithLangStags = new List<PasteWithLangStats>();
        foreach (var paste in pastes)
        {
            if (paste is Paste regularPaste)
            {
                var stats = pasteService.GetLanguageStats(regularPaste);
                pastesWithLangStags.Add(new PasteWithLangStats { Paste = paste, LanguageStats = stats });
            }
            else
            {
                pastesWithLangStags.Add(new PasteWithLangStats { Paste = paste, LanguageStats = null });
            }
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

    public async Task<List<string>> GetTagsAsync(string username, CancellationToken cancellationToken)
    {
        if (!userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to get your own tags.");

        var user = await GetByUsernameAsync(username, cancellationToken);

        if (!userContext.UserIsSelf(user))
            throw new HttpException(HttpStatusCode.Unauthorized, "You can only fetch your own tags.");

        if (!userContext.HasScope(Scope.User, Scope.UserRead))
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.UserRead.ToEnumString()}.");

        var filter = Builders<Paste>.Filter.Eq(p => p.OwnerId, user.Id);

        return (await mongo.Pastes.Find(filter).Project(p => p.Tags).ToListAsync(cancellationToken: cancellationToken))
            .SelectMany(t => t)
            .Distinct()
            .ToList();
    }

    public async Task DeleteUserAsync(string username, CancellationToken cancellationToken)
    {
        if (!userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to delete your account.");

        if (!userContext.HasScope(Scope.User))
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.User.ToEnumString()}.");

        var user = await GetByUsernameAsync(username, cancellationToken);

        if (userContext.UserIsSelf(user))
            throw new HttpException(HttpStatusCode.Unauthorized, "You can delete only your account.");

        await imageService.DeleteAsync(user.AvatarId);

        await mongo.Pastes.DeleteManyAsync(p => p.OwnerId == user.Id, cancellationToken: cancellationToken);
        await mongo.Users.DeleteOneAsync(u => u.Id == user.Id, cancellationToken: cancellationToken);

        // Delete all stars of this user
        var starsFilter = Builders<Paste>.Filter.AnyEq(p => p.Stars, user.Id);
        var starsUpdate = Builders<Paste>.Update.Pull(p => p.Stars, user.Id);
        await mongo.Pastes.UpdateManyAsync(starsFilter, starsUpdate, cancellationToken: cancellationToken);

        await actionLogger.LogActionAsync(ActionLogType.UserDeleted, user.Id);
    }
}
