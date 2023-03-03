using Microsoft.EntityFrameworkCore;
using pastemyst.DbContexts;
using pastemyst.Models;

namespace pastemyst.Services;

public interface IUserProvider
{
    public Task<User> FindByProviderAsync(string providerName, string providerId);
    public Task<bool> ExistsByUsernameAsync(string username);
    public Task<bool> ExistsById(string id);
}

public class UserProvider : IUserProvider
{
    private readonly DataContext _dbContext;

    public UserProvider(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> FindByProviderAsync(string providerName, string providerId)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(user =>
            user.ProviderName == providerName &&
            user.ProviderId == providerId
        );
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _dbContext.Users.AnyAsync(user => user.Username == username);
    }

    public async Task<bool> ExistsById(string id)
    {
        return await _dbContext.Users.AnyAsync(user => user.Id == id);
    }
}