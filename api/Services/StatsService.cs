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
            TotalUsers = await _dbContext.ActionLogs.CountAsync(l => l.Type == ActionLogType.UserCreated),
            ActivePastesOverTime = await GetActivePasteStatsOverTime(),
            TotalPastesOverTime = await GetActionLogStatsOverTime(ActionLogType.PasteCreated),
        };
    }

    private async Task<SortedDictionary<DateTime, int>> GetActionLogStatsOverTime(ActionLogType type)
    {
        var statsOverTime = await _dbContext.ActionLogs
            .Where(a => a.Type == type)
            .GroupBy(a => a.CreatedAt.Date)
            .ToDictionaryAsync(group => group.Key, group => group.Count());

        var statsOverTimeSorted = new SortedDictionary<DateTime, int>(statsOverTime);

        for (int i = 1; i < statsOverTimeSorted.Count; i++)
        {
            var prev = statsOverTimeSorted.ElementAt(i - 1);
            var cur = statsOverTimeSorted.ElementAt(i);

            statsOverTimeSorted[cur.Key] += prev.Value;
        }

        return statsOverTimeSorted;
    }

    private async Task<SortedDictionary<DateTime, int>> GetActivePasteStatsOverTime()
    {
        var statsOverTime = await _dbContext.ActionLogs
            .Where(a => a.Type == ActionLogType.PasteCreated || a.Type == ActionLogType.PasteDeleted || a.Type == ActionLogType.PasteExpired)
            .GroupBy(a => a.CreatedAt.Date)
            .ToDictionaryAsync(
                grp => grp.Key,
                grp => grp.Count(g => g.Type == ActionLogType.PasteCreated) - grp.Count(g => g.Type == ActionLogType.PasteDeleted || g.Type == ActionLogType.PasteExpired)
            );

        var statsOverTimeSorted = new SortedDictionary<DateTime, int>(statsOverTime);

        for (int i = 1; i < statsOverTimeSorted.Count; i++)
        {
            var prev = statsOverTimeSorted.ElementAt(i - 1);
            var cur = statsOverTimeSorted.ElementAt(i);

            statsOverTimeSorted[cur.Key] += prev.Value;
        }

        return statsOverTimeSorted;
    }
}
