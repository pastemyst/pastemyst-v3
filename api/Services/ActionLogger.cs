using pastemyst.DbContexts;
using pastemyst.Models;

namespace pastemyst.Services;

public interface IActionLogger
{
    public Task LogActionAsync(ActionLogType type, string objectId);
}

public class ActionLogger : IActionLogger
{
    private readonly DataContext _dbContext;

    public ActionLogger(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogActionAsync(ActionLogType type, string objectId)
    {
        var actionLog = new ActionLog
        {
            Type = type,
            ObjectId = objectId
        };

        await _dbContext.ActionLogs.AddAsync(actionLog);
        await _dbContext.SaveChangesAsync();
    }
}
