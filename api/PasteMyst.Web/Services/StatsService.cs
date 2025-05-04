using MongoDB.Bson;
using MongoDB.Driver;
using PasteMyst.Web.Models;

namespace PasteMyst.Web.Services;

public class StatsService(MongoService mongo)
{
    public async Task<AppStats> GetAppStatsAsync(CancellationToken cancellationToken)
    {
        // Explicitly cast ActionLogType enum to its integer value
        var pasteCreated = ActionLogType.PasteCreated;
        var pasteDeleted = ActionLogType.PasteDeleted;
        var pasteExpired = ActionLogType.PasteExpired;

        var userCreated = ActionLogType.UserCreated;
        var userDeleted = ActionLogType.UserDeleted;

        // 1. Count total and active pastes
        var pasteCountsTask = mongo.ActionLogs.Aggregate()
        .Match(log => log.Type == pasteCreated || log.Type == pasteDeleted || log.Type == pasteExpired) // Match with integer values
        .Group(new BsonDocument
        {
            { "_id", "$type" }, // Group by ActionLogType (stored as integer)
            { "count", new BsonDocument("$sum", 1) }
        })
        .ToListAsync(cancellationToken);

        // 2. Count total and active users
        var userCountsTask = mongo.ActionLogs.Aggregate()
            .Match(log => log.Type == userCreated || log.Type == userDeleted)
            .Group(new BsonDocument
            {
                { "_id", "$type" },
                { "count", new BsonDocument("$sum", 1) }
            })
            .ToListAsync(cancellationToken);

        // 3. Weekly paste stats
        var weeklyStatsTask = GetWeeklyPasteStatsAsync(cancellationToken);

        await Task.WhenAll(pasteCountsTask, userCountsTask, weeklyStatsTask);

        var pasteCounts = pasteCountsTask.Result;
        var userCounts = userCountsTask.Result;
        var weeklyStats = weeklyStatsTask.Result;

        Console.WriteLine(pasteCounts[0]);

        long GetCount(BsonValue type, List<BsonDocument> counts) =>
            counts.FirstOrDefault(d => d["_id"] == type)?.GetValue("count", 0).ToInt64() ?? 0;

        long totalPastes = GetCount((int)pasteCreated, pasteCounts);
        long deletedPastes = GetCount((int)pasteDeleted, pasteCounts);
        long expiredPastes = GetCount((int)pasteExpired, pasteCounts);
        long activePastes = totalPastes - deletedPastes - expiredPastes;

        long totalUsers = GetCount((int)userCreated, userCounts);
        long deletedUsers = GetCount((int)userDeleted, userCounts);
        long activeUsers = totalUsers - deletedUsers;

        return new AppStats
        {
            ActivePastes = activePastes,
            TotalPastes = totalPastes,
            ActiveUsers = activeUsers,
            TotalUsers = totalUsers,
            WeeklyPasteStats = weeklyStats
        };
    }

    private async Task<List<WeeklyPasteStats>> GetWeeklyPasteStatsAsync(CancellationToken cancellationToken)
    {
        var pasteTypes = new[] {
            (int)ActionLogType.PasteCreated,
            (int)ActionLogType.PasteDeleted,
            (int)ActionLogType.PasteExpired
        };

        var match = new BsonDocument("$match", new BsonDocument("type", new BsonDocument("$in", new BsonArray(pasteTypes))));

        var pipeline = new[]
        {
            match,

            // 1. Group by ISO week/year and type
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument
                    {
                        { "year", new BsonDocument("$isoWeekYear", "$createdAt") },
                        { "week", new BsonDocument("$isoWeek", "$createdAt") },
                        { "type", "$type" }
                    }
                },
                { "count", new BsonDocument("$sum", 1) }
            }),

            // 2. Group by week to consolidate event types
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument
                    {
                        { "year", "$_id.year" },
                        { "week", "$_id.week" }
                    }
                },
                { "events", new BsonDocument("$push", new BsonDocument
                    {
                        { "type", "$_id.type" },
                        { "count", "$count" }
                    })
                }
            }),

            // 3. Project counts and date
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { "year", "$_id.year" },
                { "week", "$_id.week" },
                { "created", BuildEventExtract((int)ActionLogType.PasteCreated) },
                { "deleted", BuildEventExtract((int)ActionLogType.PasteDeleted) },
                { "expired", BuildEventExtract((int)ActionLogType.PasteExpired) }
            }),

            // 4. Build actual date from ISO year/week
            new BsonDocument("$project", new BsonDocument
            {
                { "date", new BsonDocument("$dateFromParts", new BsonDocument
                    {
                        { "isoWeekYear", "$year" },
                        { "isoWeek", "$week" },
                        { "isoDayOfWeek", 1 } // Monday
                    })
                },
                { "created", "$created.count" },
                { "deleted", "$deleted.count" },
                { "expired", "$expired.count" }
            }),

            new BsonDocument("$sort", new BsonDocument("date", 1))
        };

        var result = await mongo.ActionLogs.Aggregate<WeeklyPasteStats>(pipeline, cancellationToken: cancellationToken).ToListAsync(cancellationToken);

        // 5. Compute cumulative totals
        int total = 0, active = 0;
        foreach (var stat in result)
        {
            total += stat.Created;
            active += stat.Created - stat.Deleted - stat.Expired;
            stat.Total = total;
            stat.Active = active;
        }

        return result;
    }

    private static BsonDocument BuildEventExtract(int typeValue)
    {
        return new BsonDocument("$ifNull", new BsonArray {
            new BsonDocument("$first", new BsonDocument("$filter", new BsonDocument
            {
                { "input", "$events" },
                { "as", "e" },
                { "cond", new BsonDocument("$eq", new BsonArray { "$$e.type", typeValue }) }
            })),
            new BsonDocument("count", 0)
        });
    }
}
