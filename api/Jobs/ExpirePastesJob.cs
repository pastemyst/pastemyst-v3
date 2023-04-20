using Microsoft.EntityFrameworkCore;
using pastemyst.DbContexts;
using pastemyst.Models;
using pastemyst.Services;
using Quartz;

namespace pastemyst.Jobs;

public class ExpirePastesJob : IJob
{
    private readonly DataContext _dbContext;
    private readonly IActionLogger _actionLogger;
    private readonly ILogger<ExpirePastesJob> _logger;

    public ExpirePastesJob(DataContext dbContext, ILogger<ExpirePastesJob> logger, IActionLogger actionLogger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _actionLogger = actionLogger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var toDelete = await _dbContext.Pastes
            .Where(p => p.DeletesAt.Value <= DateTime.UtcNow)
            .ToListAsync();

        if (toDelete.Count == 0) return;

        foreach (var paste in toDelete)
        {
            await _actionLogger.LogActionAsync(ActionLogType.PasteExpired, paste.Id);
        }

        _dbContext.RemoveRange(toDelete);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"Deleted {toDelete.Count} expired paste(s).");
    }
}
