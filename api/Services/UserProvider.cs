using System.Net;
using Microsoft.EntityFrameworkCore;
using pastemyst.DbContexts;
using pastemyst.Exceptions;
using pastemyst.Models;

namespace pastemyst.Services;

public interface IUserProvider
{
    public Task<User> GetByUsernameOrIdAsync(string username, string id);

    public Task<User> GetByUsernameAsync(string username);

    public Task<bool> ExistsByUsernameAsync(string username);

    public Task<Page<Paste>> GetOwnedPastesAsync(string username, bool pinnedOnly, PageRequest pageRequest);

    public Task<List<string>> GetTagsAsync(string username);
}

public class UserProvider : IUserProvider
{
    private readonly IUserContext _userContext;
    private readonly DataContext _dbContext;

    public UserProvider(DataContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<User> GetByUsernameOrIdAsync(string username, string id)
    {
        User user = null;

        if (username is not null)
        {
            user = await _dbContext.Users.Where(u => u.Username == username).FirstOrDefaultAsync();
        }
        else if (id is not null)
        {
            user = await _dbContext.Users.FindAsync(id);
        }

        return user;
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _dbContext.Users.Include(u => u.Settings)
            .FirstOrDefaultAsync(user => user.Username.Equals(username));
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await GetByUsernameAsync(username) is not null;
    }

    public async Task<Page<Paste>> GetOwnedPastesAsync(string username, bool pinnedOnly, PageRequest pageRequest)
    {
        var user = await GetByUsernameAsync(username);

        // If not showing only pinned pastes, and show all pastes is disabled, return an empty list.
        if (!pinnedOnly && _userContext.Self != user && !user.Settings.ShowAllPastesOnProfile)
        {
            return new Page<Paste>();
        }

        var pastesQuery = _dbContext.Pastes
            .Where(p => p.Owner == user) // check owner
            .Where(p => !p.Private || p.Owner == _userContext.Self) // only get private if self is owner
            .Where(p => !pinnedOnly || p.Pinned); // if pinnedOnly, make sure all pasted are pinned

        var pastes = pastesQuery.OrderBy(p => p.CreatedAt)
            .Reverse()
            .Include(p => p.Pasties)
            .Skip(pageRequest.Page * pageRequest.PageSize)
            .Take(pageRequest.PageSize)
            .ToList();

        var totalItems = pastesQuery.Count();
        var totalPages = (int)Math.Ceiling((float)totalItems / pageRequest.PageSize);

        if (_userContext.Self != user)
        {
            pastes.ForEach(p => p.Tags = new());
        }

        return new Page<Paste>
        {
            Items = pastes,
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

        return _dbContext.Pastes
            .Where(p => p.Owner == user)
            .ToList()
            .SelectMany(p => p.Tags ?? new List<string>())
            .Distinct()
            .ToList();
    }
}
