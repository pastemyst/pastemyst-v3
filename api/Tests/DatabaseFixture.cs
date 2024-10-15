using pastemyst.Services;

namespace pastemyst.Tests;

public class DatabaseFixture : IDisposable
{
    public MongoService MongoService { get; }

    public DatabaseFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:DefaultDb", "mongodb://localhost:27017" },
                    { "ASPNETCORE_ENVIRONMENT", "Test" }
                })
            .Build();

        MongoService = new MongoService(configuration);
    }

    public void Dispose()
    {
        MongoService.DeleteTestDatabase();
    }
}
