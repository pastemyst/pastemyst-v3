using pastemyst.Models;

namespace pastemyst.Services;

public class ActionLogger(MongoService mongo)
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
