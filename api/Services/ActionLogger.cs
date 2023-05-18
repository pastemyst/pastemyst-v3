using pastemyst.Models;

namespace pastemyst.Services;

public interface IActionLogger
{
    public Task LogActionAsync(ActionLogType type, string objectId);
}

public class ActionLogger : IActionLogger
{
    private readonly IMongoService _mongo;

    public ActionLogger(IMongoService mongo)
    {
        _mongo = mongo;
    }

    public async Task LogActionAsync(ActionLogType type, string objectId)
    {
        var actionLog = new ActionLog
        {
            Type = type,
            ObjectId = objectId
        };

        await _mongo.ActionLogs.InsertOneAsync(actionLog);
    }
}
