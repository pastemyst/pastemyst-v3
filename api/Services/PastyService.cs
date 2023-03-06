using Microsoft.EntityFrameworkCore;
using pastemyst.DbContexts;

namespace pastemyst.Services;

public interface IPastyService
{
    public Task<bool> ExistsByIdAsync(string id);
}

public class PastyService : IPastyService
{
    private readonly DataContext _dbContext;

    public PastyService(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsByIdAsync(string id)
    {
        return await _dbContext.Pasties.AnyAsync(p => p.Id == id);
    }
}