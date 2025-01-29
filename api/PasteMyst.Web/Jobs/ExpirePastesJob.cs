using MongoDB.Driver;
using PasteMyst.Web.Models;
using PasteMyst.Web.Services;
using Quartz;

namespace PasteMyst.Web.Jobs;

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
