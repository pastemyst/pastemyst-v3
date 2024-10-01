using pastemyst.Models;
using pastemyst.Services;
using Xunit;

namespace pastemyst.Tests;

public class PasteTests : IClassFixture<DatabaseFixture>
{
    private DatabaseFixture databaseFixture;
    private IIdProvider idProvider;
    private LanguageProvider languageProvider;
    private IUserContext userContext;
    private IActionLogger actionLogger;
    private IPasteService pasteService;

    public PasteTests(DatabaseFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;

        idProvider = new IdProvider();
        languageProvider = new LanguageProvider();
        userContext = new UserContext();
        actionLogger = new ActionLogger(databaseFixture.MongoService);
        pasteService = new PasteService(idProvider, languageProvider, userContext, actionLogger, databaseFixture.MongoService);

        Task.Run(() => languageProvider.StartAsync(CancellationToken.None)).Wait();
    }

    [Fact]
    public async Task TestCreateSimplePaste()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!",
                    Language = "Text"
                }
            },
        };

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.Equal("Hello, World!", paste.Pasties[0].Content);
    }
}
