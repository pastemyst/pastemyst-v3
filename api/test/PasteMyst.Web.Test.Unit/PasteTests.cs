using System.Net;
using MongoDB.Driver;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Test.Unit;

public sealed class PasteTests
{
    private DatabaseFixture databaseFixture;
    private UserContext userContext;
    private PasteService pasteService;
    private readonly EncryptionContext encryptionContext = new();

    private readonly Scope[] defaultScopes = [Scope.Paste, Scope.User];

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var idProvider = new IdProvider();
        var languageProvider = new LanguageProvider();
        var actionLogger = new ActionLogger(databaseFixture.MongoService);

        userContext = new UserContext();
        pasteService = new PasteService(idProvider, languageProvider, userContext, encryptionContext, actionLogger, databaseFixture.MongoService);

        Task.Run(() => languageProvider.StartAsync(CancellationToken.None)).Wait();
    }

    [Test]
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

        Assert.That(paste, Is.Not.Null);
        Assert.That(paste.Pasties[0].Content, Is.EqualTo("Hello, World!"));
    }

    [Test]
    public void TestCreate_ShouldThrow_WhenPinnedPaste_WhileLoggedOut()
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));
    }

    [Test]
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

        Assert.That(paste.Pinned, Is.True);
        
        userContext.LogoutUser();
    }

    [Test]
    public void TestCreate_ShouldThrow_WhenPinnedAndPrivatePaste_WhileLoggedIn()
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));

        userContext.LogoutUser();
    }

    [Test]
    public void TestCreate_ShouldThrow_WhenPinnedAndAnonymousPaste_WhileLoggedIn()
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));

        userContext.LogoutUser();
    }

    [Test]
    public void TestCreate_ShouldThrow_WhenPrivatePaste_WhileLoggedOut()
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(paste.OwnerId, Is.Null);

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(paste.Private, Is.True);

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(paste.CreatedAt.AddHours(1).Ticks, Is.EqualTo(paste.DeletesAt!.Value.Ticks));
    }

    [Test]
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

        Assert.That(paste, Is.Not.Null);
        Assert.That(paste.Pasties, Is.Not.Empty);
        Assert.That(paste.Pasties[0].Language, Is.EqualTo("Text"));
    }

    [Test]
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

        Assert.That(paste, Is.Not.Null);
        Assert.That(paste.Pasties, Is.Not.Empty);
        Assert.That(paste.Pasties[0].Language, Is.EqualTo("C#"));
    }

    [Test]
    public void TestCreate_ShouldThrow_WhenEncryptedPaste_WhenMissingEncryptionKey()
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.CreateAsync(createInfo));
    }

    [Test]
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

        Assert.That(paste, Is.Not.Null);
        Assert.That(paste.Pasties, Is.Not.Empty);
        Assert.That(paste.Pasties[0].Content, Is.EqualTo("Hello, World!"));

        encryptionContext.EncryptionKey = null;
    }

    [Test]
    public void TestGet_ShouldThrow_WhenPasteDoesntExist()
    {
        Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync("1"));
    }

    [Test]
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

        Assert.That(await pasteService.ExistsByIdAsync(paste.Id), Is.True);

        var update = Builders<Paste>.Update.Set(p => p.DeletesAt, DateTime.UtcNow.AddHours(-2));
        await databaseFixture.MongoService.Pastes.UpdateOneAsync(p => p.Id == paste.Id, update);

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(paste.Id));

        Assert.That(await pasteService.ExistsByIdAsync(paste.Id), Is.False);
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(paste.Id));
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(fetchedPaste.Tags, Is.EquivalentTo(new List<string> {"tag"}));

        userContext.LogoutUser();

        fetchedPaste = await pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Tags, Is.Empty);
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(paste.Id));
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAsync(paste.Id));

        encryptionContext.EncryptionKey = null;
    }

    [Test]
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

        Assert.That(fetchedPaste.Pasties[0].Content, Is.EqualTo("Hello, World!"));

        encryptionContext.EncryptionKey = null;
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        var exception = Assert.ThrowsAsync<HttpException>(async () => await pasteService.DeleteAsync(paste.Id));

        Assert.That(exception.Status, Is.EqualTo(HttpStatusCode.NotFound));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(await pasteService.ExistsByIdAsync(paste.Id), Is.False);

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.ToggleStarAsync(paste.Id));
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.ToggleStarAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(fetchedPaste.Stars, Contains.Item(userContext.Self.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(fetchedPaste.Stars, Does.Not.Contain(userContext.Self.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.IsStarredAsync(paste.Id));
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.IsStarredAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(isStarred, Is.False);

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(isStarred, Is.True);

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePinnedAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(fetchedPaste.Pinned, Is.True);

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(fetchedPaste.Pinned, Is.False);

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.TogglePrivateAsync(paste.Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(fetchedPaste.Private, Is.True);

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(fetchedPaste.Private, Is.False);

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(paste.Id, editInfo));
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(paste.Id, editInfo));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.EditAsync(paste.Id, editInfo));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(fetchedPaste.Title, Is.EqualTo("New Title"));
        Assert.That(fetchedPaste.Pasties[0].Content, Is.EqualTo("New Content"));

        Assert.That(fetchedPaste.History, Has.Exactly(1).Items);
        Assert.That(fetchedPaste.History[0].Title, Is.EqualTo(paste.Title));
        Assert.That(fetchedPaste.History[0].Pasties[0].Content, Is.EqualTo(paste.Pasties[0].Content));
        Assert.That(fetchedPaste.History[0].Pasties[0].Id, Is.EqualTo(fetchedPaste.Pasties[0].Id));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(fetchedPaste.Title, Is.EqualTo("New Title"));
        Assert.That(fetchedPaste.Pasties[0].Content, Is.EqualTo("New Content"));

        Assert.That(fetchedPaste.History, Has.Count.EqualTo(1));
        Assert.That(fetchedPaste.History[0].Title, Is.EqualTo(paste.Title));
        Assert.That(fetchedPaste.History[0].Pasties[0].Content, Is.EqualTo(paste.Pasties[0].Content));
        Assert.That(fetchedPaste.History[0].Pasties[0].Id, Is.EqualTo(fetchedPaste.Pasties[0].Id));

        userContext.LogoutUser();
        encryptionContext.EncryptionKey = null;
    }

    [Test]
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

        Assert.That(fetchedPaste.Tags, Is.EquivalentTo(new[] { "tag2", "tag3" }));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.ThrowsAsync<HttpException>(async () => await pasteService.GetAtEditAsync(paste.Id, "1"));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(fetchedPaste.Title, Is.EqualTo(paste.Title));
        Assert.That(fetchedPaste.Pasties[0].Content, Is.EqualTo(paste.Pasties[0].Content));

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(fetchedPaste.Title, Is.EqualTo(paste.Title));
        Assert.That(fetchedPaste.Pasties[0].Content, Is.EqualTo(paste.Pasties[0].Content));

        userContext.LogoutUser();
        encryptionContext.EncryptionKey = null;
    }

    [Test]
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

        Assert.That(diff.CurrentPaste.Title, Is.EqualTo("New Title"));
        Assert.That(diff.NewPaste.Title, Is.EqualTo("New Title"));
        Assert.That(diff.CurrentPaste.Pasties[0].Content, Is.EqualTo("New Content"));
        Assert.That(diff.NewPaste.Pasties[0].Content, Is.EqualTo("New Content"));
        Assert.That(diff.OldPaste.Pasties[0].Content, Is.EqualTo("Hello, World!"));
        Assert.That(diff.OldPaste.Title, Is.Empty);

        userContext.LogoutUser();
    }

    [Test]
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

        Assert.That(diff.CurrentPaste.Title, Is.EqualTo("New Title 2"));
        Assert.That(diff.CurrentPaste.Pasties[0].Content, Is.EqualTo("New Content 2"));

        Assert.That(diff.NewPaste.Title, Is.EqualTo("New Title 2"));
        Assert.That(diff.NewPaste.Pasties[0].Content, Is.EqualTo("New Content 2"));

        Assert.That(diff.OldPaste.Title, Is.EqualTo("New Title"));
        Assert.That(diff.OldPaste.Pasties[0].Content, Is.EqualTo("New Content"));

        userContext.LogoutUser();
    }
}
