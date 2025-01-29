using MongoDB.Driver;
using PasteMyst.Web.Models;
using PasteMyst.Web.Services;
using Quartz;

namespace PasteMyst.Web.Jobs;

public class ExpireSesssionSettingsJob(
    ILogger<ExpireSesssionSettingsJob> logger,
    MongoService mongo)
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
