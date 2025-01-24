using Microsoft.Extensions.Configuration;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Test.Unit;

public sealed class DatabaseFixture : IDisposable
{
    public MongoService MongoService { get; }

    public DatabaseFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
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
