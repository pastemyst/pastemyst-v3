using System.Net;
using MongoDB.Driver;
using pastemyst.Exceptions;
using pastemyst.Models;
using pastemyst.Services;
using Xunit;

namespace pastemyst.Tests;

public class PasteTests : IClassFixture<DatabaseFixture>
{
    private DatabaseFixture databaseFixture;
    private IdProvider idProvider;
    private LanguageProvider languageProvider;
    private UserContext userContext;
    private ActionLogger actionLogger;
    private PasteService pasteService;

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
    public async Task TestCreate_ShouldCreate_WhenSimplePaste_WhileLoggedOut()
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
    public async Task TestCreate_ShouldThrow_WhenPinnedPaste_WhileLoggedOut()
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
    public async Task TestCreate_ShouldNotThrow_WhenPinnedPaste_WhileLoggedIn()
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
    public async Task TestCreate_ShouldThrow_WhenPinnedAndPrivatePaste_WhileLoggedIn()
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
    public async Task TestCreate_ShouldThrow_WhenPinnedAndAnonymousPaste_WhileLoggedIn()
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
    public async Task TestCreate_ShouldThrow_WhenPrivatePaste_WhileLoggedOut()
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
    public async Task TestCreate_ShouldThrow_WhenPrivateAndAnonymousPaste_WhileLoggedIn()
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
    public async Task TestCreate_ShouldThrow_WhenTaggedPaste_WhileLoggedOut()
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
    public async Task TestCreate_ShouldThrow_WhenTaggedAndAnonymousPaste_WhileLoggedIn()
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
    public async Task TestCreate_ShouldSetEmptyOwner_WhenAnonymousPaste_WhileLoggedIn()
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
    public async Task TestCreate_ShouldCreatePrivatePaste_WhenPrivatePaste_WhileLoggedIn()
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
    public async Task TestCreate_ShouldSetDeletesAt_WhenSetExpiredInPaste()
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
    public async Task TestCreate_ShouldSetLanguageToText_WhenLanguageNull()
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
    public async Task TestCreate_ShouldSetProperLanguage_WhenProvidedLanguageExtension()
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
    public async Task TestGet_ShouldThrow_WhenPasteDoesntExist()
    {
        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync("1"));
    }

    [Fact]
    public async Task TestGet_ShouldThrowAndDeletePaste_WhenPasteIsExpired()
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
    public async Task TestGet_ShouldThrow_WhenPasteIsPrivateAndLoggedOut()
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
    public async Task TestGet_ShouldThrow_WhenPasteIsPrivateAndNotOwner()
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
    public async Task TestGet_ShouldReturnEmptyTags_WhenNotOwner()
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

    [Fact]
    public async Task TestDelete_ShouldThrow_WhenLoggedOut()
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));
    }

    [Fact]
    public async Task TestDelete_ShouldThrow_WhenNotOwnedPaste()
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

        userContext.LoginUser(new User { Id = "1" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestDelete_ShouldThrow_WhenNotOwner()
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

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "2" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestDelete_ShouldThrowNotFound_WhenNotOwnerAndPrivate()
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

        var exception = await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));

        Assert.Equal(HttpStatusCode.NotFound, exception.Status);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestDelete_ShouldDelete_WhenOwner()
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

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        await pasteService.DeleteAsync(paste.Id);

        Assert.False(await pasteService.ExistsByIdAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestToggleStar_ShouldThrow_WhenLoggedOut()
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.ToggleStarAsync(paste.Id));
    }

    [Fact]
    public async Task TestToggleStar_ShouldThrow_WhenPrivateAndNotOwner()
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.ToggleStarAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestToggleStar_ShouldStar_WhenNotStarred()
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

        userContext.LoginUser(new User { Id = "1" });
        await pasteService.ToggleStarAsync(paste.Id);

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.Contains(userContext.Self.Id, fetchedPaste.Stars);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestToggleStar_ShouldUnstar_WhenStarred()
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

        userContext.LoginUser(new User { Id = "1" });
        await pasteService.ToggleStarAsync(paste.Id);
        await pasteService.ToggleStarAsync(paste.Id);

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.DoesNotContain(userContext.Self.Id, fetchedPaste.Stars);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestIsStarred_ShouldThrow_WhenLoggedOut()
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.IsStarredAsync(paste.Id));
    }

    [Fact]
    public async Task TestIsStarred_ShouldThrow_WhenPrivateAndNotOwner()
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.IsStarredAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestIsStarred_ShouldReturnFalse_WhenNotStarred()
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

        userContext.LoginUser(new User { Id = "1" });

        var isStarred = await pasteService.IsStarredAsync(paste.Id);

        Assert.False(isStarred);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestIsStarred_ShouldReturnTrue_WhenStarred()
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

        userContext.LoginUser(new User { Id = "1" });
        await pasteService.ToggleStarAsync(paste.Id);

        var isStarred = await pasteService.IsStarredAsync(paste.Id);

        Assert.True(isStarred);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestPin_ShouldThrow_WhenLoggedOut()
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));
    }

    [Fact]
    public async Task TestPin_ShouldThrow_WhenNotOwnedPaste()
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

        userContext.LoginUser(new User { Id = "1" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestPin_ShouldThrow_WhenNotOwner()
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

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "2" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestPin_ShouldThrow_WhenPrivate()
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestPin_ShouldPin_WhenNotPinned()
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

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        await pasteService.TogglePinnedAsync(paste.Id);

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.True(fetchedPaste.Pinned);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestPin_ShouldUnpin_WhenPinned()
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

        await pasteService.TogglePinnedAsync(paste.Id);

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.False(fetchedPaste.Pinned);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestPrivate_ShouldThrow_WhenLoggedOut()
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));
    }

    [Fact]
    public async Task TestPrivate_ShouldThrow_WhenNotOwnedPaste()
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

        userContext.LoginUser(new User { Id = "1" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestPrivate_ShouldThrow_WhenNotOwner()
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

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "2" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestPrivate_ShouldThrow_WhenPinned()
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestPrivate_ShouldPrivate_WhenNotPrivate()
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

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        await pasteService.TogglePrivateAsync(paste.Id);

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.True(fetchedPaste.Private);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestPrivate_ShouldUnprivate_WhenPrivate()
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

        await pasteService.TogglePrivateAsync(paste.Id);

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.False(fetchedPaste.Private);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestEdit_ShouldThrow_WhenLoggedOut()
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

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties = new()
            {
                new()
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content"
                }
            }
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(paste.Id, editInfo));
    }

    [Fact]
    public async Task TestEdit_ShouldThrow_WhenNotOwnedPaste()
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

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties = new()
            {
                new()
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content"
                }
            }
        };

        userContext.LoginUser(new User { Id = "1" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(paste.Id, editInfo));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestEdit_ShouldThrow_WhenNotOwner()
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

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties = new()
            {
                new()
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content"
                }
            }
        };

        userContext.LoginUser(new User { Id = "2" });

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(paste.Id, editInfo));

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestEdit_ShouldUpdateTitleAndContent()
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

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties = new()
            {
                new()
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content"
                }
            }
        };

        await pasteService.EditAsync(paste.Id, editInfo);

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.Equal("New Title", fetchedPaste.Title);
        Assert.Equal("New Content", fetchedPaste.Pasties[0].Content);

        Assert.Single(fetchedPaste.History);
        Assert.Equal(paste.Title, fetchedPaste.History[0].Title);
        Assert.Equal(paste.Pasties[0].Content, fetchedPaste.History[0].Pasties[0].Content);
        Assert.Equal(fetchedPaste.Pasties[0].Id, fetchedPaste.History[0].Pasties[0].Id);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestEdit_ShouldUpdateTags()
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

        userContext.LoginUser(new User { Id = "1" });

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Pasties = new()
            {
                new()
                {
                    Id = paste.Pasties[0].Id,
                    Content = "Hello, World!"
                }
            },
            Tags = ["tag"]
        };

        await pasteService.EditAsync(paste.Id, editInfo);

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.Equal(paste.Title, fetchedPaste.Title);
        Assert.Equal(paste.Pasties[0].Content, fetchedPaste.Pasties[0].Content);

        Assert.Single(fetchedPaste.History);
        Assert.Equal(paste.Title, fetchedPaste.History[0].Title);
        Assert.Equal(paste.Pasties[0].Content, fetchedPaste.History[0].Pasties[0].Content);
        Assert.Equal(fetchedPaste.Pasties[0].Id, fetchedPaste.History[0].Pasties[0].Id);
        Assert.Equal(["tag"], fetchedPaste.Tags);

        userContext.LogoutUser();
    }
}
