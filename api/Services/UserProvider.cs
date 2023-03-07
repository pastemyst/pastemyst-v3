using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using pastemyst.DbContexts;
using pastemyst.Models;

namespace pastemyst.Services;

public interface IUserProvider
{
    public Task<User> GetByUsernameOrIdAsync(string username, string id);

    public Task<User> GetByUsernameAsync(string username);

    public Task<Page<Paste>> GetOwnedPastesAsync(string username, PageRequest pageRequest);
}

public class UserProvider : IUserProvider
{
    private readonly IAuthService _authService;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly DataContext _dbContext;

    public UserProvider(DataContext dbContext, IAuthService authService, IHttpContextAccessor contextAccessor)
    {
        _dbContext = dbContext;
        _authService = authService;
        _contextAccessor = contextAccessor;
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
        return await _dbContext.Users.FirstOrDefaultAsync(user => user.Username.Equals(username));
    }

    public async Task<Page<Paste>> GetOwnedPastesAsync(string username, PageRequest pageRequest)
    {
        var self = await _authService.GetSelfAsync(_contextAccessor.HttpContext);
        var user = await GetByUsernameAsync(username);

        var pastes = _dbContext.Pastes
            .Where(CanViewPaste(user, self))
            .OrderBy(p => p.CreatedAt)
            .Skip(pageRequest.Page * pageRequest.PageSize)
            .Take(pageRequest.PageSize)
            .ToList();

        var totalItems = _dbContext.Pastes.Count(CanViewPaste(user, self));
        var totalPages = (int)Math.Ceiling((float)totalItems / pageRequest.PageSize);

        return new Page<Paste>
        {
            Items = pastes,
            CurrentPage = pageRequest.Page,
            PageSize = pageRequest.PageSize,
            HasNextPage = pageRequest.Page < totalPages - 1,
            TotalPages = totalPages
        };
    }

    private static Expression<Func<Paste, bool>> CanViewPaste(User user, User self)
    {
        return p => p.Owner == user && (!p.Private || self == p.Owner);
    }
}