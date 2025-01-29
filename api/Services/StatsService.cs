using MongoDB.Bson;
using MongoDB.Driver;
using pastemyst.Models;

namespace pastemyst.Services;

public class StatsService(MongoService mongo)
{
    public async Task<AppStats> GetAppStatsAsync(CancellationToken token)
    {
        var totalPastesFilter = Builders<ActionLog>.Filter.Eq(a => a.Type, ActionLogType.PasteCreated);
        var totalUsersFilter = Builders<ActionLog>.Filter.Eq(a => a.Type, ActionLogType.UserCreated);

        return new()
        {
            ActivePastes = await mongo.Pastes.CountDocumentsAsync(new BsonDocument(), cancellationToken: token),
            ActiveUsers = await mongo.Users.CountDocumentsAsync(new BsonDocument(), cancellationToken: token),
            TotalPastes = await mongo.ActionLogs.CountDocumentsAsync(totalPastesFilter, cancellationToken: token),
            TotalUsers = await mongo.ActionLogs.CountDocumentsAsync(totalUsersFilter, cancellationToken: token),
            ActivePastesOverTime = await GetActivePasteStatsOverTime(),
            TotalPastesOverTime = await GetActionLogStatsOverTime(ActionLogType.PasteCreated),
        };
    }

    private async Task<SortedDictionary<DateTime, long>> GetActionLogStatsOverTime(ActionLogType type)
    {
        var filter = Builders<ActionLog>.Filter.Eq(a => a.Type, type);

        var statsOverTime = (await mongo.ActionLogs
            .Find(filter)
            .ToListAsync())
            .GroupBy(a => a.CreatedAt)
            .ToDictionary(g => g.Key, g => (long) g.Count());

        var statsOverTimeSorted = new SortedDictionary<DateTime, long>(statsOverTime);

        for (int i = 1; i < statsOverTimeSorted.Count; i++)
        {
            var prev = statsOverTimeSorted.ElementAt(i - 1);
            var cur = statsOverTimeSorted.ElementAt(i);

            statsOverTimeSorted[cur.Key] += prev.Value;
        }

        return statsOverTimeSorted;
    }

    private async Task<SortedDictionary<DateTime, long>> GetActivePasteStatsOverTime()
    {
        var filter = Builders<ActionLog>.Filter.Eq(a => a.Type, ActionLogType.PasteCreated) |
                     Builders<ActionLog>.Filter.Eq(a => a.Type, ActionLogType.PasteDeleted) |
                     Builders<ActionLog>.Filter.Eq(a => a.Type, ActionLogType.PasteExpired);

        var statsOverTime = (await mongo.ActionLogs
            .Find(filter)
            .ToListAsync())
            .GroupBy(a => a.CreatedAt)
            .ToDictionary(
                grp => grp.Key,
                grp => (long) grp.Count(g => g.Type == ActionLogType.PasteCreated) - grp.Count(g => g.Type == ActionLogType.PasteDeleted || g.Type == ActionLogType.PasteExpired)
            );

        var statsOverTimeSorted = new SortedDictionary<DateTime, long>(statsOverTime);

        for (int i = 1; i < statsOverTimeSorted.Count; i++)
        {
            var prev = statsOverTimeSorted.ElementAt(i - 1);
            var cur = statsOverTimeSorted.ElementAt(i);

            statsOverTimeSorted[cur.Key] += prev.Value;
        }

        return statsOverTimeSorted;
    }
}
