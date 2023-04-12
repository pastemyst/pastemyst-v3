using Microsoft.EntityFrameworkCore;
using pastemyst.DbContexts;
using pastemyst.Models;

namespace pastemyst.Services;

public interface IStatsService
{
    public Task<AppStats> GetAppStatsAsync();
}

public class StatsService : IStatsService
{
    private readonly DataContext _dbContext;

    public StatsService(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppStats> GetAppStatsAsync()
    {
        return new()
        {
            ActivePastes = await _dbContext.Pastes.CountAsync(),
            ActiveUsers = await _dbContext.Users.CountAsync(),
            TotalPastes = await _dbContext.ActionLogs.CountAsync(l => l.Type == ActionLogType.PasteCreated),
            TotalUsers = await _dbContext.ActionLogs.CountAsync(l => l.Type == ActionLogType.UserCreated)
        };
    }
}
