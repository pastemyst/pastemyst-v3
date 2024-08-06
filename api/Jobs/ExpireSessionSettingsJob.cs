using MongoDB.Driver;
using pastemyst.Models;
using pastemyst.Services;
using Quartz;

namespace pastemyst.Jobs;

public class ExpireSesssionSettingsJob(
    ILogger<ExpireSesssionSettingsJob> logger,
    IMongoService mongo)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var filter = Builders<SessionSettings>.Filter.Lt(s => s.LastAccessed, DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)));

        var toDelete = await mongo.SessionSettings.Find(filter).ToListAsync();

        if (toDelete.Count == 0) return;

        await mongo.SessionSettings.DeleteManyAsync(filter);

        logger.LogInformation($"Deleted {toDelete.Count} expired session settings.");
    }
}
