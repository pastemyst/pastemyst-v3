using MongoDB.Driver;
using pastemyst.Models;
using pastemyst.Services;
using Quartz;

namespace pastemyst.Jobs;

public class ExpirePastesJob(ILogger<ExpirePastesJob> logger, ActionLogger actionLogger, MongoService mongo)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var filter = Builders<Paste>.Filter.Lt(p => p.DeletesAt.Value, DateTime.UtcNow);

        var toDelete = await mongo.Pastes.Find(filter).ToListAsync();

        if (toDelete.Count == 0) return;

        foreach (var paste in toDelete)
        {
            await actionLogger.LogActionAsync(ActionLogType.PasteExpired, paste.Id);
        }

        await mongo.Pastes.DeleteManyAsync(filter);

        logger.LogInformation($"Deleted {toDelete.Count} expired paste(s).");
    }
}
