using MongoDB.Driver;
using pastemyst.Exceptions;
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
    public async Task Test_ShouldCreate_WhenSimplePaste_WhileLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.Equal("Hello, World!", paste.Pasties[0].Content);
    }

    [Fact]
    public async Task Test_ShouldThrow_WhenPinnedPaste_WhileLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Pinned = true,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));
    }

    [Fact]
    public async Task Test_ShouldNotThrow_WhenPinnedPaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Pinned = true,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.True(paste.Pinned);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task Test_ShouldThrow_WhenPinnedAndPrivatePaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Pinned = true,
            Private = true,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        userContext.LoginUser(new User { Id = "1" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task Test_ShouldThrow_WhenPinnedAndAnonymousPaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Pinned = true,
            Anonymous = true,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        userContext.LoginUser(new User { Id = "1" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task Test_ShouldThrow_WhenPrivatePaste_WhileLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));
    }

    [Fact]
    public async Task Test_ShouldThrow_WhenPrivateAndAnonymousPaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Anonymous = true,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        userContext.LoginUser(new User { Id = "1" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task Test_ShouldThrow_WhenTaggedPaste_WhileLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Tags = ["tag"],
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));
    }

    [Fact]
    public async Task Test_ShouldThrow_WhenTaggedAndAnonymousPaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Tags = ["epik"],
            Anonymous = true,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        userContext.LoginUser(new User { Id = "1" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task Test_ShouldSetEmptyOwner_WhenAnonymousPaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Anonymous = true,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.Null(paste.OwnerId);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task Test_ShouldCreatePrivatePaste_WhenPrivatePaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.True(paste.Private);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task Test_ShouldSetDeletesAt_WhenSetExpiredInPaste()
    {
        var createInfo = new PasteCreateInfo
        {
            ExpiresIn = ExpiresIn.OneHour,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.Equal(paste.CreatedAt.AddHours(1).Ticks, paste.DeletesAt.Value.Ticks);
    }

    [Fact]
    public async Task Test_ShouldSetLanguageToText_WhenLanguageNull()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.Equal("Text", paste.Pasties[0].Language);
    }

    [Fact]
    public async Task Test_ShouldSetProperLanguage_WhenProvidedLanguageExtension()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!",
                    Language = "cs"
                }
            },
        };

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.Equal("C#", paste.Pasties[0].Language);
    }

    [Fact]
    public async Task Test_ShouldThrow_WhenPasteDoesntExist()
    {
        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync("1"));
    }

    [Fact]
    public async Task Test_ShouldThrowAndDeletePaste_WhenPasteIsExpired()
    {
        var createInfo = new PasteCreateInfo
        {
            ExpiresIn = ExpiresIn.OneHour,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.True(await pasteService.ExistsByIdAsync(paste.Id));

        var update = Builders<Paste>.Update.Set(p => p.DeletesAt, DateTime.UtcNow.AddHours(-2));
        await databaseFixture.MongoService.Pastes.UpdateOneAsync(p => p.Id == paste.Id, update);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(paste.Id));

        Assert.False(await pasteService.ExistsByIdAsync(paste.Id));
    }

    [Fact]
    public async Task Test_ShouldThrow_WhenPasteIsPrivateAndLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LogoutUser();

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(paste.Id));
    }

    [Fact]
    public async Task Test_ShouldThrow_WhenPasteIsPrivateAndNotOwner()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "2" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task Test_ShouldReturnEmptyTags_WhenNotOwner()
    {
        var createInfo = new PasteCreateInfo
        {
            Tags = ["tag"],
            Pasties = new()
            {
                new()
                {
                    Content = "Hello, World!"
                }
            },
        };

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.Equal(["tag"], fetchedPaste.Tags);

        userContext.LogoutUser();

        fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.Empty(fetchedPaste.Tags);
    }
}
