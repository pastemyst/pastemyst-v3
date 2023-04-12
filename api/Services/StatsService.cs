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
        var activePastesOverTime = _dbContext.Pastes
            .GroupBy(p => p.CreatedAt.Date)
            .ToDictionary(group => group.Key, group => group.Count());

        var totalPastesOverTime = _dbContext.ActionLogs
            .Where(a => a.Type == ActionLogType.PasteCreated)
            .GroupBy(a => a.CreatedAt.Date)
            .ToDictionary(group => group.Key, group => group.Count());

        var activeUsersOverTime = _dbContext.Users
            .GroupBy(p => p.CreatedAt.Date)
            .ToDictionary(group => group.Key, group => group.Count());

        var totalUsersOverTime = _dbContext.ActionLogs
            .Where(a => a.Type == ActionLogType.UserCreated)
            .GroupBy(a => a.CreatedAt.Date)
            .ToDictionary(group => group.Key, group => group.Count());

        return new()
        {
            ActivePastes = await _dbContext.Pastes.CountAsync(),
            ActiveUsers = await _dbContext.Users.CountAsync(),
            TotalPastes = await _dbContext.ActionLogs.CountAsync(l => l.Type == ActionLogType.PasteCreated),
            TotalUsers = await _dbContext.ActionLogs.CountAsync(l => l.Type == ActionLogType.UserCreated),
            ActivePastesOverTime = activePastesOverTime,
            TotalPastesOverTime = totalPastesOverTime,
            ActiveUsersOverTime = activeUsersOverTime,
            TotalUsersOverTime = totalUsersOverTime
        };
    }
}
