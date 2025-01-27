using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using pastemyst.Models;
using pastemyst.Models.Auth;
using pastemyst.Serializers;

namespace pastemyst.Services;

public class MongoService
{
    public IMongoCollection<BasePaste> BasePastes { get; private set; }
    public IMongoCollection<Paste> Pastes { get; private set; }
    public IMongoCollection<EncryptedPaste> EncryptedPastes { get; private set; }

    public IMongoCollection<User> Users { get; private set; }

    public IMongoCollection<ActionLog> ActionLogs { get; private set; }

    public IMongoCollection<SessionSettings> SessionSettings { get; private set; }

    public IMongoCollection<AccessToken> AccessTokens { get; private set; }

    public GridFSBucket Images { get; private set; }

    private MongoClient mongoClient;

    public MongoService(IConfiguration configuration)
    {
        BsonSerializer.TryRegisterSerializer(new CustomEnumStringSerializer<ExpiresIn>());
        BsonSerializer.TryRegisterSerializer(new CustomEnumStringSerializer<Scope>());

        var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);

        mongoClient = new MongoClient(configuration.GetConnectionString("DefaultDb"));

        var databaseName = "pastemyst";
        if (configuration["ASPNETCORE_ENVIRONMENT"] == "Test")
            databaseName += "_test";

        var mongoDb = mongoClient.GetDatabase(databaseName);

        BasePastes = mongoDb.GetCollection<BasePaste>("pastes");
        Pastes = BasePastes.OfType<Paste>();
        EncryptedPastes = BasePastes.OfType<EncryptedPaste>();
        Users = mongoDb.GetCollection<User>("users");
        ActionLogs = mongoDb.GetCollection<ActionLog>("actionLogs");
        SessionSettings = mongoDb.GetCollection<SessionSettings>("sessionSettings");
        AccessTokens = mongoDb.GetCollection<AccessToken>("accessTokens");

        Images = new GridFSBucket(mongoDb, new()
        {
            BucketName = "images",
            ChunkSizeBytes = 1_000_000
        });

        var usernameIndex = Builders<User>.IndexKeys.Ascending(u => u.Username);

        Users.Indexes.CreateOne(new CreateIndexModel<User>(usernameIndex, new()
        {
            Unique = true
        }));
    }

    public void DeleteTestDatabase()
    {
        mongoClient.DropDatabase("pastemyst_test");
    }
}
