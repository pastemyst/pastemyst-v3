using System.Net;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using pastemyst.Exceptions;
using pastemyst.Models;

namespace pastemyst.Services;

public interface IUserProvider
{
    public Task<User> GetByUsernameOrIdAsync(string username, string id);

    public Task<User> GetByUsernameAsync(string username);

    public Task<bool> ExistsByUsernameAsync(string username);

    public Task<Page<PasteWithLangStats>> GetOwnedPastesAsync(string username, string tag, bool pinnedOnly, PageRequest pageRequest);

    public Task<List<string>> GetTagsAsync(string username);
}

public class UserProvider : IUserProvider
{
    private readonly IUserContext _userContext;
    private readonly IPasteService _pasteService;
    private readonly IMongoService _mongo;

    public UserProvider(IUserContext userContext, IPasteService pasteService, IMongoService mongo)
    {
        _userContext = userContext;
        _pasteService = pasteService;
        _mongo = mongo;
    }

    public async Task<User> GetByUsernameOrIdAsync(string username, string id)
    {
        User user = null;

        if (username is not null)
        {
            return await GetByUsernameAsync(username);
        }
        else if (id is not null)
        {
            user = await _mongo.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        return user;
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Username, username);
        return await _mongo.Users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await GetByUsernameAsync(username) is not null;
    }

    public async Task<Page<PasteWithLangStats>> GetOwnedPastesAsync(string username, string tag, bool pinnedOnly, PageRequest pageRequest)
    {
        var user = await GetByUsernameAsync(username);

        // If not showing only pinned pastes, and show all pastes is disabled, return an empty list.
        if (!pinnedOnly && _userContext.Self != user && !user.Settings.ShowAllPastesOnProfile)
        {
            return new Page<PasteWithLangStats>();
        }

        var filter = Builders<Paste>.Filter.Eq(p => p.OwnerId, user.Id) &
                     (Builders<Paste>.Filter.Eq(p => p.Private, false) | Builders<Paste>.Filter.Eq(p => p.OwnerId, _userContext.Self.Id));

        if (pinnedOnly) filter &= Builders<Paste>.Filter.Eq(p => p.Pinned, true);

        if (tag is not null)
        {
            if (_userContext.Self != user)
            {
                throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to view paste tags.");
            }

            filter &= Builders<Paste>.Filter.ElemMatch(p => p.Tags, tag);
        }

        var pastes = await _mongo.Pastes.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip(pageRequest.Page * pageRequest.PageSize)
            .Limit(pageRequest.PageSize)
            .ToListAsync();

        var totalItems = await _mongo.Pastes.CountDocumentsAsync(filter);
        var totalPages = (int)Math.Ceiling((float)totalItems / pageRequest.PageSize);

        if (_userContext.Self != user)
        {
            pastes.ForEach(p => p.Tags = new());
        }

        var pastesWithLangStags = new List<PasteWithLangStats>();
        foreach (var paste in pastes)
        {
            var stats = _pasteService.GetLanguageStats(paste);

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
        if (!_userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to get your own tags.");

        var user = await GetByUsernameAsync(username);

        if (_userContext.Self != user)
            throw new HttpException(HttpStatusCode.Unauthorized, "You can only fetch your own tags.");

        var filter = Builders<Paste>.Filter.Eq(p => p.OwnerId, user.Id);

        return (await _mongo.Pastes.Find(filter).Project(p => p.Tags).ToListAsync())
            .SelectMany(t => t)
            .Distinct()
            .ToList();
    }
}
