using pastemyst.Models;

namespace pastemyst.Services;

public interface IActionLogger
{
    public Task LogActionAsync(ActionLogType type, string objectId);
}

public class ActionLogger(IMongoService mongo) : IActionLogger
{
    public async Task LogActionAsync(ActionLogType type, string objectId)
    {
        var actionLog = new ActionLog
        {
            Type = type,
            ObjectId = objectId
        };

        await mongo.ActionLogs.InsertOneAsync(actionLog);
    }
}
