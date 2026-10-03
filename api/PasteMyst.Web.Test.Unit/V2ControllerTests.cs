using Microsoft.Extensions.Configuration;
using PasteMyst.Web.Controllers.V2;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Models.V2;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Test.Unit;

/// <summary>
/// Locks the v2 API contract that existing integrations depend on. The v2 controllers are thin
/// translation layers over v3 services, so they're constructed directly and their returned v2
/// models are asserted (no HTTP layer needed).
/// </summary>
public sealed class V2ControllerTests
{
    private readonly DatabaseFixture _fixture = new();
    private readonly UserContext _userContext = new();
    private readonly EncryptionContext _encryptionContext = new();

    private PasteService _pasteService;
    private LanguageProvider _languageProvider;
    private PasteControllerV2 _pasteController;
    private DataControllerV2 _dataController;

    private static readonly Scope[] DefaultScopes = [Scope.Paste, Scope.User];

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var idProvider = new IdProvider();
        _languageProvider = new LanguageProvider();
        var actionLogger = new ActionLogger(_fixture.MongoService);

        _pasteService = new PasteService(idProvider, _languageProvider, _userContext, _encryptionContext, actionLogger, _fixture.MongoService);

        Task.Run(() => _languageProvider.StartAsync(CancellationToken.None)).Wait();

        _pasteController = new PasteControllerV2(_pasteService);
        _dataController = new DataControllerV2(_languageProvider, _pasteService);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _fixture.Dispose();
    }

    [TearDown]
    public void TearDown()
    {
        _userContext.LogoutUser();
    }

    private static PasteCreateInfoV2 SimpleCreateInfo(string tags = "") => new()
    {
        Title = "my title",
        ExpiresIn = ExpiresIn.Never,
        Tags = tags,
        Pasties =
        [
            new PastyCreateInfoV2 { Title = "pasty", Language = "Text", Code = "hello v2" }
        ]
    };

    [Test]
    public async Task CreateThenGetPaste_ReturnsV2Shape()
    {
        var created = await _pasteController.CreatePaste(SimpleCreateInfo(), CancellationToken.None);
        var fetched = await _pasteController.GetPaste(created.Id);

        Assert.Multiple(() =>
        {
            // v2 uses "code", not "content".
            Assert.That(fetched.Pasties[0].Code, Is.EqualTo("hello v2"));
            Assert.That(fetched.Title, Is.EqualTo("my title"));
            // Stars is an int count in v2, not the v3 user-id list.
            Assert.That(fetched.Stars, Is.EqualTo(0));
            // Edits are always empty in v2 (v3 handles history differently).
            Assert.That(fetched.Edits, Is.Empty);
            Assert.That(fetched.IsPrivate, Is.False);
            Assert.That(fetched.IsPublic, Is.False);
            // CreatedAt is unix seconds; DeletesAt is 0 for a never-expiring paste.
            Assert.That(fetched.CreatedAt, Is.GreaterThan(0));
            Assert.That(fetched.DeletesAt, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task CreatePaste_LoggedIn_TranslatesCommaSeparatedTags()
    {
        _userContext.LoginUser(new User { Id = "tagger" }, DefaultScopes);

        var created = await _pasteController.CreatePaste(SimpleCreateInfo(tags: "a, b"), CancellationToken.None);
        var fetched = await _pasteController.GetPaste(created.Id);

        // v2 sends tags as a comma-separated string; v3 stores a trimmed list.
        Assert.That(fetched.Tags, Is.EquivalentTo(new[] { "a", "b" }));
    }

    [Test]
    public async Task GetPaste_ReturnsStarsAsIntCount()
    {
        _userContext.LoginUser(new User { Id = "starrer" }, DefaultScopes);

        var created = await _pasteController.CreatePaste(SimpleCreateInfo(), CancellationToken.None);
        await _pasteService.ToggleStarAsync(created.Id);

        var fetched = await _pasteController.GetPaste(created.Id);

        Assert.That(fetched.Stars, Is.EqualTo(1));
    }

    [Test]
    public async Task DeletePaste_RemovesPaste()
    {
        _userContext.LoginUser(new User { Id = "deleter" }, DefaultScopes);

        var created = await _pasteController.CreatePaste(SimpleCreateInfo(), CancellationToken.None);

        await _pasteController.DeletePaste(created.Id);

        Assert.ThrowsAsync<HttpException>(async () => await _pasteController.GetPaste(created.Id));
    }

    [Test]
    public async Task DataController_NumPastes_CountsEncryptedPastes()
    {
        var before = (await _dataController.GetNumPastes(CancellationToken.None)).NumPastes;

        await _fixture.MongoService.EncryptedPastes.InsertOneAsync(new EncryptedPaste
        {
            Id = "numenc01",
            CreatedAt = DateTime.UtcNow,
            EncryptedData = "",
            Iv = "",
            Salt = ""
        });

        var after = (await _dataController.GetNumPastes(CancellationToken.None)).NumPastes;

        Assert.That(after, Is.EqualTo(before + 1));
    }

    [Test]
    public void TimeController_Never_ReturnsZero()
    {
        var controller = new TimeControllerV2();
        var result = controller.ExpiresInToUnixTime(createdAt: 0, expiresIn: "never");
        Assert.That(result.Result, Is.EqualTo(0));
    }

    [Test]
    public void TimeController_OneHour_AddsAnHourInSeconds()
    {
        var controller = new TimeControllerV2();
        var result = controller.ExpiresInToUnixTime(createdAt: 0, expiresIn: "1h");
        Assert.That(result.Result, Is.EqualTo(3600));
    }

    [Test]
    public void DataController_GetLanguageByName_ReturnsV2LanguageWithBareExtensions()
    {
        var lang = _dataController.GetLanguageByName("JavaScript");

        Assert.Multiple(() =>
        {
            Assert.That(lang.Name, Is.EqualTo("JavaScript"));
            // v2 returns extensions without the leading dot.
            Assert.That(lang.Ext, Does.Contain("js"));
        });
    }

    [Test]
    public async Task UserController_GetUser_RewritesAvatarUrlToV3ImagesEndpoint()
    {
        var user = new User
        {
            Id = "avataruser",
            Username = "avataruser",
            AvatarId = "avatar123",
            UserSettings = new UserSettings { ShowAllPastesOnProfile = true },
            Settings = new Settings()
        };
        await _fixture.MongoService.Users.InsertOneAsync(user);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "Host", "https://paste.myst.rs" } })
            .Build();

        var userProvider = new UserProvider(_userContext, _pasteService, _fixture.MongoService, actionLogger: null, imageService: null);
        var controller = new UserControllerV2(userProvider, configuration, _userContext);

        var result = await controller.GetUser("avataruser", CancellationToken.None);

        Assert.That(result.AvatarUrl, Is.EqualTo("https://paste.myst.rs/api/v3/images/avatar123"));
    }
}
