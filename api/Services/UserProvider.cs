using System.Net;
using System.Security.Claims;
using MongoDB.Driver;
using OpenIddict.Abstractions;
using pastemyst.Exceptions;
using pastemyst.Extensions;
using pastemyst.Models;

namespace pastemyst.Services;

public class UserProvider(PasteService pasteService, MongoService mongo, ActionLogger actionLogger, ImageService imageService)
{
    public async Task<User> GetSelfAsync(ClaimsPrincipal self)
    {
        var id = self.GetClaim(ClaimTypes.NameIdentifier);
        var selfUser = await mongo.Users.Find(u => u.Id == id).FirstAsync();

        return selfUser;
    }
    
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

    public async Task<Page<PasteWithLangStats>> GetOwnedPastesAsync(ClaimsPrincipal self, string username, string tag, bool pinnedOnly, PageRequest pageRequest)
    {
        var user = await GetByUsernameAsync(username) ?? throw new HttpException(HttpStatusCode.NotFound, "User not found.");

        // If not showing only pinned pastes, and show all pastes is disabled, return an empty list.
        if (!pinnedOnly && self.Id() != user.Id && !user.UserSettings.ShowAllPastesOnProfile)
        {
            return new Page<PasteWithLangStats>();
        }

        var filter = Builders<Paste>.Filter.Eq(p => p.OwnerId, user.Id);

        if (self.Id() != user.Id)
        {
            filter &= Builders<Paste>.Filter.Eq(p => p.Private, false);
        }

        if (pinnedOnly) filter &= Builders<Paste>.Filter.Eq(p => p.Pinned, true);

        if (tag is not null)
        {
            if (self.Id() != user.Id)
            {
                throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to view paste tags.");
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
        if (self.Id() != user.Id)
        {
            pastes.ForEach(p => p.Tags = []);
        }

        var pastesWithLangStags = new List<PasteWithLangStats>();
        foreach (var paste in pastes)
        {
            var stats = pasteService.GetLanguageStats(paste);

            pastesWithLangStags.Add(new PasteWithLangStats { Paste = paste, LanguageStats = stats });
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

    public async Task<List<string>> GetTagsAsync(ClaimsPrincipal self, string username)
    {
        if (!self.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to get your own tags.");

        var user = await GetByUsernameAsync(username);

        if (self.Id() != user.Id)
            throw new HttpException(HttpStatusCode.Unauthorized, "You can only fetch your own tags.");

        var filter = Builders<Paste>.Filter.Eq(p => p.OwnerId, user.Id);

        return (await mongo.Pastes.Find(filter).Project(p => p.Tags).ToListAsync())
            .SelectMany(t => t)
            .Distinct()
            .ToList();
    }

    public async Task DeleteUserAsync(ClaimsPrincipal self, string username)
    {
        if (self.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to delete your account.");

        var user = await GetByUsernameAsync(username);

        if (self.Id() != user.Id)
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
}
