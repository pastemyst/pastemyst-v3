using System.Net;
using MongoDB.Driver;
using pastemyst.Exceptions;
using pastemyst.Models;
using pastemyst.Models.Auth;
using pastemyst.Services;
using Xunit;

namespace pastemyst.Tests;

public class PasteTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture databaseFixture;
    private readonly UserContext userContext;
    private readonly PasteService pasteService;
    private readonly EncryptionContext encryptionContext = new();

    private readonly Scope[] defaultScopes = [Scope.Paste, Scope.User];

    public PasteTests(DatabaseFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;

        var idProvider = new IdProvider();
        var languageProvider = new LanguageProvider();
        var actionLogger = new ActionLogger(databaseFixture.MongoService);

        userContext = new UserContext();
        pasteService = new PasteService(idProvider, languageProvider, userContext, encryptionContext, actionLogger, databaseFixture.MongoService);

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

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.Equal("Hello, World!", paste!.Pasties[0].Content);
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));

        userContext.LogoutUser();
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));

        userContext.LogoutUser();
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

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.Equal(paste.CreatedAt.AddHours(1).Ticks, paste.DeletesAt!.Value.Ticks);
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

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.Equal("Text", paste!.Pasties[0].Language);
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

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.Equal("C#", paste!.Pasties[0].Language);
    }

    [Fact]
    public async Task TestCreate_ShouldThrow_WhenEncryptedPaste_WhenMissingEncryptionKey()
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
            Encrypted = true
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));
    }

    [Fact]
    public async Task TestCreate_ShouldCreate_WhenEncryptedPaste()
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
            Encrypted = true
        };

        encryptionContext.EncryptionKey = "epikepik";

        var paste = await pasteService.CreateAsync(createInfo);

        Assert.Equal("Hello, World!", paste.Pasties[0].Content);

        encryptionContext.EncryptionKey = null;
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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "2" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(paste.Id));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.Equal(["tag"], fetchedPaste.Tags);

        userContext.LogoutUser();

        fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.Empty(fetchedPaste.Tags);
    }

    [Fact]
    public async Task TestGet_ShouldThrow_WhenPasteIsEncrypted_WhenEncryptionKeyMissing()
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
            Encrypted = true
        };

        encryptionContext.EncryptionKey = "epikepik";

        var paste = await pasteService.CreateAsync(createInfo);

        encryptionContext.EncryptionKey = null;

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(paste.Id));
    }

    [Fact]
    public async Task TestGet_ShouldThrow_WhenPasteIsEncrypted_WhenEncryptionKeyWrong()
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
            Encrypted = true
        };

        encryptionContext.EncryptionKey = "epikepik";

        var paste = await pasteService.CreateAsync(createInfo);

        encryptionContext.EncryptionKey = "epik";

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(paste.Id));

        encryptionContext.EncryptionKey = null;
    }

    [Fact]
    public async Task TestGet_ShouldReturn_WhenPasteIsEncrypted()
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
            Encrypted = true
        };

        encryptionContext.EncryptionKey = "epikepik";

        var paste = await pasteService.CreateAsync(createInfo);

        encryptionContext.EncryptionKey = "epikepik";

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.Equal("Hello, World!", fetchedPaste.Pasties[0].Content);

        encryptionContext.EncryptionKey = null;
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

        var paste = await pasteService.CreateAsync(createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));
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

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "2" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "2" }, defaultScopes);

        var exception = await Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));

        Assert.Equal(HttpStatusCode.NotFound, exception.Status);

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "2" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.ToggleStarAsync(paste.Id));

        userContext.LogoutUser();
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

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);
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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);
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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "2" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.IsStarredAsync(paste.Id));

        userContext.LogoutUser();
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

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var isStarred = await pasteService.IsStarredAsync(paste.Id);

        Assert.False(isStarred);

        userContext.LogoutUser();
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

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);
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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));
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

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "2" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));
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

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        userContext.LoginUser(new User { Id = "2" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

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
            Pasties =
            [
                new PastyCreateInfo
                {
                    Content = "Hello, World!"
                }
            ],
        };

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste!.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(paste.Id, editInfo));
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

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste!.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(paste.Id, editInfo));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste!.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        userContext.LoginUser(new User { Id = "2" }, defaultScopes);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(paste.Id, editInfo));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste!.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
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
    public async Task TestEdit_ShouldUpdateContent_WhenEncrypted()
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
            Encrypted = true
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);
        encryptionContext.EncryptionKey = "epikepik";

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste!.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
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
        encryptionContext.EncryptionKey = null;
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        await pasteService.EditTagsAsync(paste.Id, ["tag2", "tag3"]);

        var fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.Equal(["tag2", "tag3"], fetchedPaste.Tags);

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        await Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAtEditAsync(paste.Id, "1"));

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste!.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await pasteService.EditAsync(paste.Id, editInfo);

        var history = await pasteService.GetHistoryCompactAsync(paste.Id);

        var fetchedPaste = await pasteService.GetAtEditAsync(paste.Id, history[0].Id);

        Assert.Equal(paste.Title, fetchedPaste.Title);
        Assert.Equal(paste.Pasties[0].Content, fetchedPaste.Pasties[0].Content);

        userContext.LogoutUser();
    }

    [Fact]
    public async Task TestGetAtEdit_ShouldReturnPasteAtEdit_WhenEncrypted()
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
            Encrypted = true
        };

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);
        encryptionContext.EncryptionKey = "epikepik";

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste!.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await pasteService.EditAsync(paste.Id, editInfo);

        var history = await pasteService.GetHistoryCompactAsync(paste.Id);

        var fetchedPaste = await pasteService.GetAtEditAsync(paste.Id, history[0].Id);

        Assert.Equal(paste.Title, fetchedPaste.Title);
        Assert.Equal(paste.Pasties[0].Content, fetchedPaste.Pasties[0].Content);

        userContext.LogoutUser();
        encryptionContext.EncryptionKey = null;
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste!.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await pasteService.EditAsync(paste.Id, editInfo);

        var history = await pasteService.GetHistoryCompactAsync(paste.Id);

        var diff = await pasteService.GetDiffAsync(paste.Id, history[0].Id);

        Assert.Equal("New Title", diff.CurrentPaste.Title);
        Assert.Equal("New Title", diff.NewPaste.Title);
        Assert.Equal("New Content", diff.CurrentPaste.Pasties[0].Content);
        Assert.Equal("New Content", diff.NewPaste.Pasties[0].Content);
        Assert.Equal("Hello, World!", diff.OldPaste.Pasties[0].Content);
        Assert.Empty(diff.OldPaste.Title);

        userContext.LogoutUser();
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

        userContext.LoginUser(new User { Id = "1" }, defaultScopes);

        var paste = await pasteService.CreateAsync(createInfo);

        var editInfo = new PasteEditInfo
        {
            Title = "New Title",
            Pasties =
            [
                new PastyEditInfo
                {
                    Id = paste!.Pasties[0].Id,
                    Content = "New Content"
                }
            ]
        };

        await pasteService.EditAsync(paste.Id, editInfo);

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

        await pasteService.EditAsync(paste.Id, editInfo);

        var history = await pasteService.GetHistoryCompactAsync(paste.Id);

        var diff = await pasteService.GetDiffAsync(paste.Id, history[0].Id);

        Assert.Equal("New Title 2", diff.CurrentPaste.Title);
        Assert.Equal("New Content 2", diff.CurrentPaste.Pasties[0].Content);

        Assert.Equal("New Title 2", diff.NewPaste.Title);
        Assert.Equal("New Content 2", diff.NewPaste.Pasties[0].Content);

        Assert.Equal("New Title", diff.OldPaste.Title);
        Assert.Equal("New Content", diff.OldPaste.Pasties[0].Content);

        userContext.LogoutUser();
    }
}
