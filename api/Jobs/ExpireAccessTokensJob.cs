using MongoDB.Driver;
using pastemyst.Models;
using pastemyst.Models.Auth;
using pastemyst.Services;
using Quartz;

namespace pastemyst.Jobs;

public class ExpireAccessTokensJob(ILogger<ExpireAccessTokensJob> logger, ActionLogger actionLogger, MongoService mongo)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var filter = Builders<AccessToken>.Filter.Lt(p => p.ExpiresAt.Value, DateTime.UtcNow);

        var toDelete = await mongo.AccessTokens.Find(filter).ToListAsync();

        if (toDelete.Count == 0) return;

        foreach (var accessToken in toDelete)
        {
            await actionLogger.LogActionAsync(ActionLogType.AccessTokenExpired, accessToken.OwnerId);
        }

        await mongo.AccessTokens.DeleteManyAsync(filter);

        logger.LogInformation($"Deleted {toDelete.Count} expired access token(s).");
    }
}
