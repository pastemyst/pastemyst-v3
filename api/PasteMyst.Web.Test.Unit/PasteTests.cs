using System.Net;
using MongoDB.Driver;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Test.Unit;

public sealed class PasteTests
{
    private readonly DatabaseFixture _databaseFixture = new();
    private readonly UserContext _userContext = new();
    private readonly EncryptionContext _encryptionContext = new();
    private PasteService _pasteService;

    private readonly Scope[] _defaultScopes = [Scope.Paste, Scope.User];

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var idProvider = new IdProvider();
        var languageProvider = new LanguageProvider();
        var actionLogger = new ActionLogger(_databaseFixture.MongoService);

        _pasteService = new PasteService(idProvider, languageProvider, _userContext, _encryptionContext, actionLogger, _databaseFixture.MongoService);

        Task.Run(() => languageProvider.StartAsync(CancellationToken.None)).Wait();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _databaseFixture.Dispose();
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.CreateAsync(createInfo, CancellationToken.None));
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.That(paste.Pinned, Is.True);
        
        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.CreateAsync(createInfo, CancellationToken.None));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.CreateAsync(createInfo, CancellationToken.None));

        _userContext.LogoutUser();
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

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.CreateAsync(createInfo, CancellationToken.None));
    }

    [Test]
    public void TestCreate_ShouldThrow_WhenPrivateAndAnonymousPaste_WhileLoggedIn()
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.CreateAsync(createInfo, CancellationToken.None));

        _userContext.LogoutUser();
    }

    [Test]
    public void TestCreate_ShouldThrow_WhenTaggedPaste_WhileLoggedOut()
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

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.CreateAsync(createInfo, CancellationToken.None));
    }

    [Test]
    public void TestCreate_ShouldThrow_WhenTaggedAndAnonymousPaste_WhileLoggedIn()
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.CreateAsync(createInfo, CancellationToken.None));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.That(paste.OwnerId, Is.Null);

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.That(paste.Private, Is.True);

        _userContext.LogoutUser();
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.CreateAsync(createInfo, CancellationToken.None));
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

        _encryptionContext.EncryptionKey = "epikepik";

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.That(paste, Is.Not.Null);
        Assert.That(paste.Pasties, Is.Not.Empty);
        Assert.That(paste.Pasties[0].Content, Is.EqualTo("Hello, World!"));

        _encryptionContext.EncryptionKey = null;
    }

    [Test]
    public void TestGet_ShouldThrow_WhenPasteDoesntExist()
    {
        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.GetAsync("1"));
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.That(await _pasteService.ExistsByIdAsync(paste.Id), Is.True);

        var update = Builders<Paste>.Update.Set(p => p.DeletesAt, DateTime.UtcNow.AddHours(-2));
        await _databaseFixture.MongoService.Pastes.UpdateOneAsync(p => p.Id == paste.Id, update);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.GetAsync(paste.Id));

        Assert.That(await _pasteService.ExistsByIdAsync(paste.Id), Is.False);
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LogoutUser();

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.GetAsync(paste.Id));
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "2" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.GetAsync(paste.Id));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        var fetchedPaste = await _pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Tags, Is.EquivalentTo(new List<string> {"tag"}));

        _userContext.LogoutUser();

        fetchedPaste = await _pasteService.GetAsync(paste.Id);

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

        _encryptionContext.EncryptionKey = "epikepik";

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _encryptionContext.EncryptionKey = null;

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.GetAsync(paste.Id));
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

        _encryptionContext.EncryptionKey = "epikepik";

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _encryptionContext.EncryptionKey = "epik";

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.GetAsync(paste.Id));

        _encryptionContext.EncryptionKey = null;
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

        _encryptionContext.EncryptionKey = "epikepik";

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _encryptionContext.EncryptionKey = "epikepik";

        var fetchedPaste = await _pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Pasties[0].Content, Is.EqualTo("Hello, World!"));

        _encryptionContext.EncryptionKey = null;
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.DeleteAsync(paste.Id));
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.DeleteAsync(paste.Id));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "2" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.DeleteAsync(paste.Id));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "2" }, _defaultScopes);

        var exception = Assert.ThrowsAsync<HttpException>(async () => await _pasteService.DeleteAsync(paste.Id));

        Assert.That(exception.Status, Is.EqualTo(HttpStatusCode.NotFound));

        _userContext.LogoutUser();
    }

    [Test]
    public async Task TestDelete_ShouldThrowNotFound_WhenAdminAndPrivate()
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "2", IsAdmin = true }, _defaultScopes);

        var exception = Assert.ThrowsAsync<HttpException>(async () => await _pasteService.DeleteAsync(paste.Id));

        Assert.That(exception.Status, Is.EqualTo(HttpStatusCode.NotFound));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        await _pasteService.DeleteAsync(paste.Id);

        Assert.That(await _pasteService.ExistsByIdAsync(paste.Id), Is.False);

        _userContext.LogoutUser();
    }

    [Test]
    public async Task TestDelete_ShouldDelete_WhenAdmin()
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "2", IsAdmin = true }, _defaultScopes);

        await _pasteService.DeleteAsync(paste.Id);

        Assert.That(await _pasteService.ExistsByIdAsync(paste.Id), Is.False);

        _userContext.LogoutUser();
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.ToggleStarAsync(paste.Id));
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "2" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.ToggleStarAsync(paste.Id));

        _userContext.LogoutUser();
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);
        await _pasteService.ToggleStarAsync(paste.Id);

        var fetchedPaste = await _pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Stars, Contains.Item(_userContext.Self.Id));

        _userContext.LogoutUser();
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);
        await _pasteService.ToggleStarAsync(paste.Id);
        await _pasteService.ToggleStarAsync(paste.Id);

        var fetchedPaste = await _pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Stars, Does.Not.Contain(_userContext.Self.Id));

        _userContext.LogoutUser();
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.IsStarredAsync(paste.Id));
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "2" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.IsStarredAsync(paste.Id));

        _userContext.LogoutUser();
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var isStarred = await _pasteService.IsStarredAsync(paste.Id);

        Assert.That(isStarred, Is.False);

        _userContext.LogoutUser();
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);
        await _pasteService.ToggleStarAsync(paste.Id);

        var isStarred = await _pasteService.IsStarredAsync(paste.Id);

        Assert.That(isStarred, Is.True);

        _userContext.LogoutUser();
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.TogglePinnedAsync(paste.Id));
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.TogglePinnedAsync(paste.Id));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "2" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.TogglePinnedAsync(paste.Id));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.TogglePinnedAsync(paste.Id));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        await _pasteService.TogglePinnedAsync(paste.Id);

        var fetchedPaste = await _pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Pinned, Is.True);

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        await _pasteService.TogglePinnedAsync(paste.Id);

        var fetchedPaste = await _pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Pinned, Is.False);

        _userContext.LogoutUser();
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.TogglePrivateAsync(paste.Id));
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.TogglePrivateAsync(paste.Id));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        _userContext.LoginUser(new User { Id = "2" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.TogglePrivateAsync(paste.Id));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.TogglePrivateAsync(paste.Id));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        await _pasteService.TogglePrivateAsync(paste.Id);

        var fetchedPaste = await _pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Private, Is.True);

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        await _pasteService.TogglePrivateAsync(paste.Id);

        var fetchedPaste = await _pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Private, Is.False);

        _userContext.LogoutUser();
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.EditAsync(paste.Id, editInfo, CancellationToken.None));
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

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.EditAsync(paste.Id, editInfo, CancellationToken.None));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        _userContext.LoginUser(new User { Id = "2" }, _defaultScopes);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.EditAsync(paste.Id, editInfo, CancellationToken.None));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        await _pasteService.EditAsync(paste.Id, editInfo, CancellationToken.None);

        var fetchedPaste = await _pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Title, Is.EqualTo("New Title"));
        Assert.That(fetchedPaste.Pasties[0].Content, Is.EqualTo("New Content"));

        Assert.That(fetchedPaste.History, Has.Exactly(1).Items);
        Assert.That(fetchedPaste.History[0].Title, Is.EqualTo(paste.Title));
        Assert.That(fetchedPaste.History[0].Pasties[0].Content, Is.EqualTo(paste.Pasties[0].Content));
        Assert.That(fetchedPaste.History[0].Pasties[0].Id, Is.EqualTo(fetchedPaste.Pasties[0].Id));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);
        _encryptionContext.EncryptionKey = "epikepik";

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        await _pasteService.EditAsync(paste.Id, editInfo, CancellationToken.None);

        var fetchedPaste = await _pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Title, Is.EqualTo("New Title"));
        Assert.That(fetchedPaste.Pasties[0].Content, Is.EqualTo("New Content"));

        Assert.That(fetchedPaste.History, Has.Count.EqualTo(1));
        Assert.That(fetchedPaste.History[0].Title, Is.EqualTo(paste.Title));
        Assert.That(fetchedPaste.History[0].Pasties[0].Content, Is.EqualTo(paste.Pasties[0].Content));
        Assert.That(fetchedPaste.History[0].Pasties[0].Id, Is.EqualTo(fetchedPaste.Pasties[0].Id));

        _userContext.LogoutUser();
        _encryptionContext.EncryptionKey = null;
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        await _pasteService.EditTagsAsync(paste.Id, ["tag2", "tag3"]);

        var fetchedPaste = await _pasteService.GetAsync(paste.Id);

        Assert.That(fetchedPaste.Tags, Is.EquivalentTo(new[] { "tag2", "tag3" }));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteService.GetAtEditAsync(paste.Id, "1"));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        await _pasteService.EditAsync(paste.Id, editInfo, CancellationToken.None);

        var history = await _pasteService.GetHistoryCompactAsync(paste.Id);

        var fetchedPaste = await _pasteService.GetAtEditAsync(paste.Id, history[0].Id);

        Assert.That(fetchedPaste.Title, Is.EqualTo(paste.Title));
        Assert.That(fetchedPaste.Pasties[0].Content, Is.EqualTo(paste.Pasties[0].Content));

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);
        _encryptionContext.EncryptionKey = "epikepik";

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        await _pasteService.EditAsync(paste.Id, editInfo, CancellationToken.None);

        var history = await _pasteService.GetHistoryCompactAsync(paste.Id);

        var fetchedPaste = await _pasteService.GetAtEditAsync(paste.Id, history[0].Id);

        Assert.That(fetchedPaste.Title, Is.EqualTo(paste.Title));
        Assert.That(fetchedPaste.Pasties[0].Content, Is.EqualTo(paste.Pasties[0].Content));

        _userContext.LogoutUser();
        _encryptionContext.EncryptionKey = null;
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        await _pasteService.EditAsync(paste.Id, editInfo, CancellationToken.None);

        var history = await _pasteService.GetHistoryCompactAsync(paste.Id);

        var diff = await _pasteService.GetDiffAsync(paste.Id, history[0].Id);

        Assert.That(diff.CurrentPaste.Title, Is.EqualTo("New Title"));
        Assert.That(diff.NewPaste.Title, Is.EqualTo("New Title"));
        Assert.That(diff.CurrentPaste.Pasties[0].Content, Is.EqualTo("New Content"));
        Assert.That(diff.NewPaste.Pasties[0].Content, Is.EqualTo("New Content"));
        Assert.That(diff.OldPaste.Pasties[0].Content, Is.EqualTo("Hello, World!"));
        Assert.That(diff.OldPaste.Title, Is.Empty);

        _userContext.LogoutUser();
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

        _userContext.LoginUser(new User { Id = "1" }, _defaultScopes);

        var paste = await _pasteService.CreateAsync(createInfo, CancellationToken.None);

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

        await _pasteService.EditAsync(paste.Id, editInfo, CancellationToken.None);

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

        await _pasteService.EditAsync(paste.Id, editInfo, CancellationToken.None);

        var history = await _pasteService.GetHistoryCompactAsync(paste.Id);

        var diff = await _pasteService.GetDiffAsync(paste.Id, history[0].Id);

        Assert.That(diff.CurrentPaste.Title, Is.EqualTo("New Title 2"));
        Assert.That(diff.CurrentPaste.Pasties[0].Content, Is.EqualTo("New Content 2"));

        Assert.That(diff.NewPaste.Title, Is.EqualTo("New Title 2"));
        Assert.That(diff.NewPaste.Pasties[0].Content, Is.EqualTo("New Content 2"));

        Assert.That(diff.OldPaste.Title, Is.EqualTo("New Title"));
        Assert.That(diff.OldPaste.Pasties[0].Content, Is.EqualTo("New Content"));

        _userContext.LogoutUser();
    }
}
