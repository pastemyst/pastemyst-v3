using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using pastemyst.Models;
using pastemyst.Services;
using Quartz;

namespace pastemyst.Jobs;

public class ExpirePastesJob : IJob
{
    private readonly IMongoService _mongo;
    private readonly IActionLogger _actionLogger;
    private readonly ILogger<ExpirePastesJob> _logger;

    public ExpirePastesJob(ILogger<ExpirePastesJob> logger, IActionLogger actionLogger, IMongoService mongo)
    {
        _logger = logger;
        _actionLogger = actionLogger;
        _mongo = mongo;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var filter = Builders<Paste>.Filter.Lt(p => p.DeletesAt.Value, DateTime.UtcNow);

        var toDelete = await _mongo.Pastes.Find(filter).ToListAsync();

        if (toDelete.Count == 0) return;

        foreach (var paste in toDelete)
        {
            await _actionLogger.LogActionAsync(ActionLogType.PasteExpired, paste.Id);
        }

        await _mongo.Pastes.DeleteManyAsync(filter);

        _logger.LogInformation($"Deleted {toDelete.Count} expired paste(s).");
    }
}
