using System.Net;
using System.Security.Claims;
using MongoDB.Driver;
using pastemyst.Exceptions;
using pastemyst.Extensions;
using pastemyst.Models;
using pastemyst.Services;
using Xunit;

namespace pastemyst.Tests;

public class PasteTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture databaseFixture;
    private readonly IdProvider idProvider;
    private readonly LanguageProvider languageProvider;
    private readonly ActionLogger actionLogger;
    private readonly PasteService pasteService;

    private readonly ClaimsPrincipal loggedOutUser;
    private readonly ClaimsPrincipal loggedInUser;
    private readonly ClaimsPrincipal loggedInUser2;

    public PasteTests(DatabaseFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;

        idProvider = new IdProvider();
        languageProvider = new LanguageProvider();
        actionLogger = new ActionLogger(databaseFixture.MongoService);
        pasteService = new PasteService(idProvider, languageProvider, actionLogger, databaseFixture.MongoService);

        loggedOutUser = new ClaimsPrincipal();
        
        loggedInUser = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.Name, "epik_user"),
            new(ClaimTypes.NameIdentifier, "1"),
        }, "authType"));
        
        loggedInUser2 = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.Name, "not_epik_user"),
            new(ClaimTypes.NameIdentifier, "2"),
        }, "authType"));

        Task.Run(() => languageProvider.StartAsync(CancellationToken.None)).Wait();
    }

    [Fact]
    public async Task TestCreate_ShouldCreate_WhenSimplePaste_WhileLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        Assert.Equal("Hello, World!", paste.Pasties[0].Content);
    }

    [Fact]
    public async Task TestCreate_ShouldThrow_WhenPinnedPaste_WhileLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Pinned = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(loggedOutUser, createInfo));
    }

    [Fact]
    public async Task TestCreate_ShouldNotThrow_WhenPinnedPaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Pinned = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        Assert.True(paste.Pinned);
    }

    [Fact]
    public async Task TestCreate_ShouldThrow_WhenPinnedAndPrivatePaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Pinned = true,
            Private = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(loggedInUser, createInfo));
    }

    [Fact]
    public async Task TestCreate_ShouldThrow_WhenPinnedAndAnonymousPaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Pinned = true,
            Anonymous = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(loggedInUser, createInfo));
    }

    [Fact]
    public async Task TestCreate_ShouldThrow_WhenPrivatePaste_WhileLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(loggedOutUser, createInfo));
    }

    [Fact]
    public async Task TestCreate_ShouldThrow_WhenPrivateAndAnonymousPaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Anonymous = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(loggedInUser, createInfo));
    }

    [Fact]
    public async Task TestCreate_ShouldThrow_WhenTaggedPaste_WhileLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Tags = ["tag"],
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(loggedOutUser, createInfo));
    }

    [Fact]
    public async Task TestCreate_ShouldThrow_WhenTaggedAndAnonymousPaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Tags = ["epik"],
            Anonymous = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(loggedInUser, createInfo));
    }

    [Fact]
    public async Task TestCreate_ShouldSetEmptyOwner_WhenAnonymousPaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Anonymous = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        Assert.Null(paste.OwnerId);
    }

    [Fact]
    public async Task TestCreate_ShouldCreatePrivatePaste_WhenPrivatePaste_WhileLoggedIn()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        Assert.True(paste.Private);
    }

    [Fact]
    public async Task TestCreate_ShouldSetDeletesAt_WhenSetExpiredInPaste()
    {
        var createInfo = new PasteCreateInfo
        {
            ExpiresIn = ExpiresIn.OneHour,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        Assert.Equal(paste.CreatedAt.AddHours(1).Ticks, paste.DeletesAt.Value.Ticks);
    }

    [Fact]
    public async Task TestCreate_ShouldSetLanguageToText_WhenLanguageNull()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        Assert.Equal("Text", paste.Pasties[0].Language);
    }

    [Fact]
    public async Task TestCreate_ShouldSetProperLanguage_WhenProvidedLanguageExtension()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!",
                    Language = "cs"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        Assert.Equal("C#", paste.Pasties[0].Language);
    }

    [Fact]
    public async Task TestGet_ShouldThrow_WhenPasteDoesntExist()
    {
        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(loggedOutUser, "1"));
    }

    [Fact]
    public async Task TestGet_ShouldThrowAndDeletePaste_WhenPasteIsExpired()
    {
        var createInfo = new PasteCreateInfo
        {
            ExpiresIn = ExpiresIn.OneHour,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        Assert.True(await pasteService.ExistsByIdAsync(paste.Id));

        var update = Builders<Paste>.Update.Set(p => p.DeletesAt, DateTime.UtcNow.AddHours(-2));
        await databaseFixture.MongoService.Pastes.UpdateOneAsync(p => p.Id == paste.Id, update);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(loggedOutUser, paste.Id));

        Assert.False(await pasteService.ExistsByIdAsync(paste.Id));
    }

    [Fact]
    public async Task TestGet_ShouldThrow_WhenPasteIsPrivateAndLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(loggedOutUser, paste.Id));
    }

    [Fact]
    public async Task TestGet_ShouldThrow_WhenPasteIsPrivateAndNotOwner()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(loggedInUser2, paste.Id));
    }

    [Fact]
    public async Task TestGet_ShouldReturnEmptyTags_WhenNotOwner()
    {
        var createInfo = new PasteCreateInfo
        {
            Tags = ["tag"],
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        var fetchedPaste = await pasteService.GetAsync(loggedInUser, paste.Id);

        Assert.Equal(["tag"], fetchedPaste.Tags);

        fetchedPaste = await pasteService.GetAsync(loggedOutUser, paste.Id);

        Assert.Empty(fetchedPaste.Tags);
    }

    [Fact]
    public async Task TestDelete_ShouldThrow_WhenLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(loggedOutUser, paste.Id));
    }

    [Fact]
    public async Task TestDelete_ShouldThrow_WhenNotOwnedPaste()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(loggedInUser, paste.Id));
    }

    [Fact]
    public async Task TestDelete_ShouldThrow_WhenNotOwner()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(loggedInUser2, paste.Id));
    }

    [Fact]
    public async Task TestDelete_ShouldThrowNotFound_WhenNotOwnerAndPrivate()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        var exception = await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(loggedInUser2, paste.Id));

        Assert.Equal(HttpStatusCode.NotFound, exception.Status);
    }

    [Fact]
    public async Task TestDelete_ShouldDelete_WhenOwner()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await pasteService.DeleteAsync(loggedInUser, paste.Id);

        Assert.False(await pasteService.ExistsByIdAsync(paste.Id));
    }

    [Fact]
    public async Task TestToggleStar_ShouldThrow_WhenLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.ToggleStarAsync(loggedOutUser, paste.Id));
    }

    [Fact]
    public async Task TestToggleStar_ShouldThrow_WhenPrivateAndNotOwner()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.ToggleStarAsync(loggedInUser2, paste.Id));
    }

    [Fact]
    public async Task TestToggleStar_ShouldStar_WhenNotStarred()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);
        
        await pasteService.ToggleStarAsync(loggedInUser, paste.Id);

        var fetchedPaste = await pasteService.GetAsync(loggedInUser, paste.Id);

        Assert.Contains(loggedInUser.Id(), fetchedPaste.Stars);
    }

    [Fact]
    public async Task TestToggleStar_ShouldUnstar_WhenStarred()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        await pasteService.ToggleStarAsync(loggedInUser, paste.Id);
        await pasteService.ToggleStarAsync(loggedInUser, paste.Id);

        var fetchedPaste = await pasteService.GetAsync(loggedInUser, paste.Id);

        Assert.DoesNotContain(loggedInUser.Id(), fetchedPaste.Stars);
    }

    [Fact]
    public async Task TestIsStarred_ShouldThrow_WhenLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.IsStarredAsync(loggedOutUser, paste.Id));
    }

    [Fact]
    public async Task TestIsStarred_ShouldThrow_WhenPrivateAndNotOwner()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.IsStarredAsync(loggedInUser2, paste.Id));
    }

    [Fact]
    public async Task TestIsStarred_ShouldReturnFalse_WhenNotStarred()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        var isStarred = await pasteService.IsStarredAsync(loggedInUser, paste.Id);

        Assert.False(isStarred);
    }

    [Fact]
    public async Task TestIsStarred_ShouldReturnTrue_WhenStarred()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        await pasteService.ToggleStarAsync(loggedInUser, paste.Id);

        var isStarred = await pasteService.IsStarredAsync(loggedInUser, paste.Id);

        Assert.True(isStarred);
    }

    [Fact]
    public async Task TestPin_ShouldThrow_WhenLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(loggedOutUser, paste.Id));
    }

    [Fact]
    public async Task TestPin_ShouldThrow_WhenNotOwnedPaste()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(loggedInUser, paste.Id));
    }

    [Fact]
    public async Task TestPin_ShouldThrow_WhenNotOwner()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(loggedInUser2, paste.Id));
    }

    [Fact]
    public async Task TestPin_ShouldThrow_WhenPrivate()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(loggedInUser, paste.Id));
    }

    [Fact]
    public async Task TestPin_ShouldPin_WhenNotPinned()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await pasteService.TogglePinnedAsync(loggedInUser, paste.Id);

        var fetchedPaste = await pasteService.GetAsync(loggedInUser, paste.Id);

        Assert.True(fetchedPaste.Pinned);
    }

    [Fact]
    public async Task TestPin_ShouldUnpin_WhenPinned()
    {
        var createInfo = new PasteCreateInfo
        {
            Pinned = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await pasteService.TogglePinnedAsync(loggedInUser, paste.Id);

        var fetchedPaste = await pasteService.GetAsync(loggedInUser, paste.Id);

        Assert.False(fetchedPaste.Pinned);
    }

    [Fact]
    public async Task TestPrivate_ShouldThrow_WhenLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(loggedOutUser, paste.Id));
    }

    [Fact]
    public async Task TestPrivate_ShouldThrow_WhenNotOwnedPaste()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(loggedInUser, paste.Id));
    }

    [Fact]
    public async Task TestPrivate_ShouldThrow_WhenNotOwner()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(loggedInUser2, paste.Id));
    }

    [Fact]
    public async Task TestPrivate_ShouldThrow_WhenPinned()
    {
        var createInfo = new PasteCreateInfo
        {
            Pinned = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(loggedInUser, paste.Id));
    }

    [Fact]
    public async Task TestPrivate_ShouldPrivate_WhenNotPrivate()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await pasteService.TogglePrivateAsync(loggedInUser, paste.Id);

        var fetchedPaste = await pasteService.GetAsync(loggedInUser, paste.Id);

        Assert.True(fetchedPaste.Private);
    }

    [Fact]
    public async Task TestPrivate_ShouldUnprivate_WhenPrivate()
    {
        var createInfo = new PasteCreateInfo
        {
            Private = true,
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await pasteService.TogglePrivateAsync(loggedInUser, paste.Id);

        var fetchedPaste = await pasteService.GetAsync(loggedInUser, paste.Id);

        Assert.False(fetchedPaste.Private);
    }

    [Fact]
    public async Task TestEdit_ShouldThrow_WhenLoggedOut()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(loggedOutUser, paste.Id, editInfo));
    }

    [Fact]
    public async Task TestEdit_ShouldThrow_WhenNotOwnedPaste()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedOutUser, createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(loggedInUser, paste.Id, editInfo));
    }

    [Fact]
    public async Task TestEdit_ShouldThrow_WhenNotOwner()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(loggedInUser2, paste.Id, editInfo));
    }

    [Fact]
    public async Task TestEdit_ShouldUpdateTitleAndContent()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await pasteService.EditAsync(loggedInUser, paste.Id, editInfo);

        var fetchedPaste = await pasteService.GetAsync(loggedInUser, paste.Id);

        Assert.Equal("New Title", fetchedPaste.Title);
        Assert.Equal("New Content", fetchedPaste.Pasties[0].Content);

        Assert.Single(fetchedPaste.History);
        Assert.Equal(paste.Title, fetchedPaste.History[0].Title);
        Assert.Equal(paste.Pasties[0].Content, fetchedPaste.History[0].Pasties[0].Content);
        Assert.Equal(fetchedPaste.Pasties[0].Id, fetchedPaste.History[0].Pasties[0].Id);
    }

    [Fact]
    public async Task TestEditTags_ShouldUpdateTags()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
            Tags = ["tag1"]
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await pasteService.EditTagsAsync(loggedInUser, paste.Id, ["tag2", "tag3"]);

        var fetchedPaste = await pasteService.GetAsync(loggedInUser, paste.Id);

        Assert.Equal(["tag2", "tag3"], fetchedPaste.Tags);
    }

    [Fact]
    public async Task TestGetAtEdit_ShouldReturnNotFound_WhenNoHistory()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAtEditAsync(loggedInUser, paste.Id, "1"));
    }

    [Fact]
    public async Task TestGetAtEdit_ShouldReturnPasteAtEdit()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await pasteService.EditAsync(loggedInUser, paste.Id, editInfo);

        var history = await pasteService.GetHistoryCompactAsync(loggedInUser, paste.Id);

        var fetchedPaste = await pasteService.GetAtEditAsync(loggedInUser, paste.Id, history[0].Id);

        Assert.Equal(paste.Title, fetchedPaste.Title);
        Assert.Equal(paste.Pasties[0].Content, fetchedPaste.Pasties[0].Content);
    }

    [Fact]
    public async Task GetDiff_ShouldReturnProperDiff_WhenOnlyOneEdit()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await pasteService.EditAsync(loggedInUser, paste.Id, editInfo);

        var history = await pasteService.GetHistoryCompactAsync(loggedInUser, paste.Id);

        var diff = await pasteService.GetDiffAsync(loggedInUser, paste.Id, history[0].Id);

        Assert.Equal("New Title", diff.CurrentPaste.Title);
        Assert.Equal("New Title", diff.NewPaste.Title);
        Assert.Equal("New Content", diff.CurrentPaste.Pasties[0].Content);
        Assert.Equal("New Content", diff.NewPaste.Pasties[0].Content);
        Assert.Equal("Hello, World!", diff.OldPaste.Pasties[0].Content);
        Assert.Empty(diff.OldPaste.Title);
    }

    [Fact]
    public async Task GetDiff_ShouldReturnProperDiff_WhenMultipleEdits()
    {
        var createInfo = new PasteCreateInfo
        {
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(loggedInUser, createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await pasteService.EditAsync(loggedInUser, paste.Id, editInfo);

        editInfo = new PasteEditInfo
        {
            Title = "New Title 2",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste.Pasties[0].Id,
                    Content = "New Content 2"
                }
            ]
        };

        await pasteService.EditAsync(loggedInUser, paste.Id, editInfo);

        var history = await pasteService.GetHistoryCompactAsync(loggedInUser, paste.Id);

        var diff = await pasteService.GetDiffAsync(loggedInUser, paste.Id, history[0].Id);

        Assert.Equal("New Title 2", diff.CurrentPaste.Title);
        Assert.Equal("New Content 2", diff.CurrentPaste.Pasties[0].Content);

        Assert.Equal("New Title 2", diff.NewPaste.Title);
        Assert.Equal("New Content 2", diff.NewPaste.Pasties[0].Content);

        Assert.Equal("New Title", diff.OldPaste.Title);
        Assert.Equal("New Content", diff.OldPaste.Pasties[0].Content);
    }
}
