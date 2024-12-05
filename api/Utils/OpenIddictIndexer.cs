using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OpenIddict.MongoDb;
using OpenIddict.MongoDb.Models;

namespace pastemyst.Utils;

public static class OpenIddictIndexer
{
    public static void IndexOpenIddictDb(MongoClient mongoClient, IOptionsMonitor<OpenIddictMongoDbOptions> options)
    {
        var database = mongoClient.GetDatabase("openiddict");

        var applications = database.GetCollection<OpenIddictMongoDbApplication>(options.CurrentValue.ApplicationsCollectionName);

        applications.Indexes.CreateMany(
        [
            new CreateIndexModel<OpenIddictMongoDbApplication>(
                Builders<OpenIddictMongoDbApplication>.IndexKeys.Ascending(application => application.ClientId),
                new CreateIndexOptions
                {
                    Unique = true
                }),

            new CreateIndexModel<OpenIddictMongoDbApplication>(
                Builders<OpenIddictMongoDbApplication>.IndexKeys.Ascending(application => application.PostLogoutRedirectUris),
                new CreateIndexOptions
                {
                    Background = true
                }),

            new CreateIndexModel<OpenIddictMongoDbApplication>(
                Builders<OpenIddictMongoDbApplication>.IndexKeys.Ascending(application => application.RedirectUris),
                new CreateIndexOptions
                {
                    Background = true
                })
        ]);

        var authorizations = database.GetCollection<OpenIddictMongoDbAuthorization>(options.CurrentValue.AuthorizationsCollectionName);

        authorizations.Indexes.CreateOne(
            new CreateIndexModel<OpenIddictMongoDbAuthorization>(
                Builders<OpenIddictMongoDbAuthorization>.IndexKeys
                    .Ascending(authorization => authorization.ApplicationId)
                    .Ascending(authorization => authorization.Scopes)
                    .Ascending(authorization => authorization.Status)
                    .Ascending(authorization => authorization.Subject)
                    .Ascending(authorization => authorization.Type),
                new CreateIndexOptions
                {
                    Background = true
                }));

        var scopes = database.GetCollection<OpenIddictMongoDbScope>(options.CurrentValue.ScopesCollectionName);

        scopes.Indexes.CreateOne(new CreateIndexModel<OpenIddictMongoDbScope>(
            Builders<OpenIddictMongoDbScope>.IndexKeys.Ascending(scope => scope.Name),
            new CreateIndexOptions
            {
                Unique = true
            }));

        var tokens = database.GetCollection<OpenIddictMongoDbToken>(options.CurrentValue.TokensCollectionName);

        tokens.Indexes.CreateMany(
        [
            new CreateIndexModel<OpenIddictMongoDbToken>(
                Builders<OpenIddictMongoDbToken>.IndexKeys.Ascending(token => token.ReferenceId),
                new CreateIndexOptions<OpenIddictMongoDbToken>
                {
                    // Note: partial filter expressions are not supported on Azure Cosmos DB.
                    // As a workaround, the expression and the unique constraint can be removed.
                    PartialFilterExpression = Builders<OpenIddictMongoDbToken>.Filter.Exists(token => token.ReferenceId),
                    Unique = true
                }),

            new CreateIndexModel<OpenIddictMongoDbToken>(
                Builders<OpenIddictMongoDbToken>.IndexKeys.Ascending(token => token.AuthorizationId),
                new CreateIndexOptions<OpenIddictMongoDbToken>()
                {
                    PartialFilterExpression =
                        Builders<OpenIddictMongoDbToken>.Filter.Exists(token => token.AuthorizationId),
                }),

            new CreateIndexModel<OpenIddictMongoDbToken>(
                Builders<OpenIddictMongoDbToken>.IndexKeys
                    .Ascending(token => token.ApplicationId)
                    .Ascending(token => token.Status)
                    .Ascending(token => token.Subject)
                    .Ascending(token => token.Type),
                new CreateIndexOptions
                {
                    Background = true
                })
        ]);
    }
}
