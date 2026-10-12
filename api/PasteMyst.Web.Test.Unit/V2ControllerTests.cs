using MongoDB.Driver;
using System.Net;
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

        _pasteController = new PasteControllerV2(_pasteService, _userContext);
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

    // --- v2 parity: behaviour the v2 API had that clients may rely on ---

    private async Task<PasteV2> CreateOwnedPaste(string ownerId, bool isPrivate = false, string tags = "")
    {
        _userContext.LoginUser(new User { Id = ownerId }, DefaultScopes);
        var info = SimpleCreateInfo(tags);
        return await _pasteController.CreatePaste(new PasteCreateInfoV2
        {
            Title = info.Title,
            ExpiresIn = info.ExpiresIn,
            Tags = info.Tags,
            IsPrivate = isPrivate,
            Pasties =
            [
                new PastyCreateInfoV2 { Title = "one", Language = "Text", Code = "first" },
                new PastyCreateInfoV2 { Title = "two", Language = "Text", Code = "second" }
            ]
        }, CancellationToken.None);
    }

    [Test]
    public async Task EditPaste_TitleOnly_LeavesEverythingElseUnchanged()
    {
        var created = await CreateOwnedPaste("editor1", isPrivate: true, tags: "a, b");

        var edited = await _pasteController.EditPaste(created.Id, new PasteEditInfoV2 { Title = "renamed" }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(edited.Title, Is.EqualTo("renamed"));
            Assert.That(edited.IsPrivate, Is.True);
            Assert.That(edited.Tags, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(edited.Pasties.Select(p => p.Code), Is.EqualTo(new[] { "first", "second" }));
        });
    }

    [Test]
    public async Task EditPaste_PrivacyOnly_LeavesTitleAndTagsUnchanged()
    {
        var created = await CreateOwnedPaste("editor2", isPrivate: true, tags: "keep");

        var edited = await _pasteController.EditPaste(created.Id, new PasteEditInfoV2 { IsPrivate = false }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(edited.IsPrivate, Is.False);
            Assert.That(edited.Title, Is.EqualTo("my title"));
            Assert.That(edited.Tags, Is.EqualTo(new[] { "keep" }));
        });
    }

    [Test]
    public async Task EditPaste_Pasties_EditsKeptRemovesMissingAddsEmptyId()
    {
        var created = await CreateOwnedPaste("editor3");
        var first = created.Pasties[0];

        var edited = await _pasteController.EditPaste(created.Id, new PasteEditInfoV2
        {
            Pasties =
            [
                new PastyEditInfoV2 { Id = first.Id, Title = first.Title, Language = "Text", Code = "edited" },
                new PastyEditInfoV2 { Id = "", Title = "new", Language = "Text", Code = "added" }
            ]
        }, CancellationToken.None);

        Assert.That(edited.Pasties.Select(p => p.Code), Is.EqualTo(new[] { "edited", "added" }));
    }

    [Test]
    public void EditInfo_BindsPastyIdFromUnderscoreId()
    {
        // what ASP.NET uses for request bodies; v2 clients send "_id"
        var json = """{"pasties":[{"_id":"keepme12","title":"t","code":"c","language":"Text"}]}""";

        var info = System.Text.Json.JsonSerializer.Deserialize<PasteEditInfoV2>(json,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

        Assert.That(info!.Pasties[0].Id, Is.EqualTo("keepme12"));
    }

    [Test]
    public async Task EditPaste_PrivateAndPublic_Returns400()
    {
        var created = await CreateOwnedPaste("editor4");

        var e = Assert.ThrowsAsync<HttpException>(async () =>
            await _pasteController.EditPaste(created.Id, new PasteEditInfoV2 { IsPrivate = true, IsPublic = true }, CancellationToken.None));

        Assert.That(e!.Status, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task EditAndDelete_SomeoneElsesPaste_Return404()
    {
        var created = await CreateOwnedPaste("owner5");
        _userContext.LoginUser(new User { Id = "intruder5" }, DefaultScopes);

        var edit = Assert.ThrowsAsync<HttpException>(async () =>
            await _pasteController.EditPaste(created.Id, new PasteEditInfoV2 { Title = "x" }, CancellationToken.None));
        var delete = Assert.ThrowsAsync<HttpException>(async () => await _pasteController.DeletePaste(created.Id));

        Assert.Multiple(() =>
        {
            Assert.That(edit!.Status, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(delete!.Status, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task EditAndDelete_NotLoggedIn_Return404()
    {
        var created = await CreateOwnedPaste("owner6");
        _userContext.LogoutUser();

        var edit = Assert.ThrowsAsync<HttpException>(async () =>
            await _pasteController.EditPaste(created.Id, new PasteEditInfoV2 { Title = "x" }, CancellationToken.None));
        var delete = Assert.ThrowsAsync<HttpException>(async () => await _pasteController.DeletePaste(created.Id));

        Assert.Multiple(() =>
        {
            Assert.That(edit!.Status, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(delete!.Status, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    // v3 stores its own name, the v2 response shows v2's (strict v2 clients parse names into an enum)
    [TestCase("Vue.js Component", "Vue", "Vue.js Component")]
    [TestCase("Plain Text", "Text", "Plain Text")]
    [TestCase("C#", "C#", "C#")]
    public async Task CreatePaste_MapsV2LanguageNamesBothWays(string v2Language, string storedV3, string returnedV2)
    {
        var created = await _pasteController.CreatePaste(new PasteCreateInfoV2
        {
            Pasties = [new PastyCreateInfoV2 { Title = "", Language = v2Language, Code = "x" }]
        }, CancellationToken.None);

        var stored = await _pasteService.GetAsync(created.Id);

        Assert.Multiple(() =>
        {
            Assert.That(stored.Pasties[0].Language, Is.EqualTo(storedV3));
            Assert.That(created.Pasties[0].Language, Is.EqualTo(returnedV2));
        });
    }

    [Test]
    public void DataController_GetLanguageByName_AcceptsAndReturnsV2Names()
    {
        Assert.That(_dataController.GetLanguageByName("Plain Text").Name, Is.EqualTo("Plain Text"));
    }

    [Test]
    public async Task UserController_Exists_ReturnsJsonEmptyString()
    {
        await _fixture.MongoService.Users.InsertOneAsync(new User
        {
            Id = "existsuser",
            Username = "existsuser",
            UserSettings = new UserSettings(),
            Settings = new Settings()
        });

        var userProvider = new UserProvider(_userContext, _pasteService, _fixture.MongoService, actionLogger: null, imageService: null);
        var controller = new UserControllerV2(userProvider, new ConfigurationBuilder().Build(), _userContext);

        var result = await controller.GetUserExists("existsuser", CancellationToken.None);

        // v2 answered with the JSON string "", which clients read as JSON
        Assert.That(result.Value, Is.EqualTo(""));
    }

    [Test]
    public async Task DeletePaste_RespondsLikeV2()
    {
        var created = await CreateOwnedPaste("deleter7");

        var result = await _pasteController.DeletePaste(created.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.ContentType, Is.EqualTo("application/octet-stream"));
            Assert.That(result.Content, Is.Empty);
        });
    }

    [TestCase("lookupuser.one", true)]   // exact
    [TestCase("LOOKUPUSER.ONE", true)]   // case-insensitive
    [TestCase("lookupuser", false)]      // a prefix used to match
    [TestCase("ookupuser.on", false)]    // so did a substring
    [TestCase("lookupuserXone", false)]  // "." used to be a wildcard
    [TestCase("lookupuser.*", false)]
    public async Task UserLookup_MatchesWholeNameOnly(string query, bool found)
    {
        if (!await _fixture.MongoService.Users.Find(u => u.Id == "lookupuser1").AnyAsync())
        {
            await _fixture.MongoService.Users.InsertOneAsync(new User
            {
                Id = "lookupuser1", Username = "lookupuser.one", UserSettings = new UserSettings(), Settings = new Settings()
            });
        }

        var userProvider = new UserProvider(_userContext, _pasteService, _fixture.MongoService, actionLogger: null, imageService: null);

        Assert.That(await userProvider.ExistsByUsernameAsync(query, CancellationToken.None), Is.EqualTo(found));
    }

    [Test]
    public void CreatePaste_UnknownLanguage_Returns400()
    {
        var e = Assert.ThrowsAsync<HttpException>(async () => await _pasteController.CreatePaste(new PasteCreateInfoV2
        {
            Pasties = [new PastyCreateInfoV2 { Title = "", Language = "NotALanguage", Code = "x" }]
        }, CancellationToken.None));

        Assert.That(e!.Status, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public void CreatePaste_AccountFeaturesWithoutAuth_Returns403()
    {
        var e = Assert.ThrowsAsync<HttpException>(async () =>
            await _pasteController.CreatePaste(SimpleCreateInfo(tags: "a"), CancellationToken.None));

        Assert.That(e!.Status, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public void TimeController_InvalidExpiresIn_Returns400()
    {
        var e = Assert.Throws<HttpException>(() => new TimeControllerV2().ExpiresInToUnixTime(createdAt: 0, expiresIn: "bogus"));

        Assert.That(e!.Status, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public void UserController_SelfWithoutKey_Returns400()
    {
        var userProvider = new UserProvider(_userContext, _pasteService, _fixture.MongoService, actionLogger: null, imageService: null);
        var controller = new UserControllerV2(userProvider, new ConfigurationBuilder().Build(), _userContext);

        var self = Assert.ThrowsAsync<HttpException>(async () => await controller.GetSelf());
        var pastes = Assert.ThrowsAsync<HttpException>(async () => await controller.GetSelfPastes());

        Assert.Multiple(() =>
        {
            Assert.That(self!.Status, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(pastes!.Status, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    public async Task EncryptedPaste_V2_ReportsEncrypted_AndAppearsInSelfPastesAndStars()
    {
        var owner = new User { Id = "encowner1", Username = "encowner1", UserSettings = new UserSettings(), Settings = new Settings() };
        await _fixture.MongoService.Users.InsertOneAsync(owner);
        _userContext.LoginUser(owner, DefaultScopes);

        _encryptionContext.EncryptionKey = "v2-encrypted-key";
        var created = await _pasteService.CreateAsync(new PasteCreateInfo
        {
            Title = "secret",
            Encrypted = true,
            Pasties = [new PastyCreateInfo { Title = "", Content = "hidden", Language = "Text" }]
        }, CancellationToken.None);
        await _pasteService.ToggleStarAsync(created.Id);

        var fetched = await _pasteController.GetPaste(created.Id);
        var plain = await _pasteController.CreatePaste(SimpleCreateInfo(), CancellationToken.None);
        _encryptionContext.EncryptionKey = null;

        var userProvider = new UserProvider(_userContext, _pasteService, _fixture.MongoService, actionLogger: null, imageService: null);
        var userController = new UserControllerV2(userProvider, new ConfigurationBuilder().Build(), _userContext);
        var selfPastes = await userController.GetSelfPastes();
        var self = await userController.GetSelf();

        Assert.Multiple(() =>
        {
            Assert.That(fetched.Encrypted, Is.True, "an encrypted paste says so");
            Assert.That(plain.Encrypted, Is.False);
            Assert.That(selfPastes, Does.Contain(created.Id), "self/pastes includes encrypted pastes");
            Assert.That(self.Stars, Does.Contain(created.Id), "stars include encrypted pastes");
        });
    }

    [Test]
    public async Task Starring_RequiresUserScope()
    {
        _userContext.LoginUser(new User { Id = "starscope1" }, [Scope.Paste]);
        var created = await _pasteController.CreatePaste(SimpleCreateInfo(), CancellationToken.None);

        var toggle = Assert.ThrowsAsync<HttpException>(async () => await _pasteService.ToggleStarAsync(created.Id));
        var check = Assert.ThrowsAsync<HttpException>(async () => await _pasteService.IsStarredAsync(created.Id));

        _userContext.LoginUser(new User { Id = "starscope1" }, [Scope.UserRead]);
        var checkWithRead = await _pasteService.IsStarredAsync(created.Id);
        var toggleWithRead = Assert.ThrowsAsync<HttpException>(async () => await _pasteService.ToggleStarAsync(created.Id));

        _userContext.LoginUser(new User { Id = "starscope1" }, [Scope.User]);
        await _pasteService.ToggleStarAsync(created.Id);
        var starred = await _pasteService.IsStarredAsync(created.Id);

        Assert.Multiple(() =>
        {
            Assert.That(toggle!.Status, Is.EqualTo(HttpStatusCode.Forbidden), "paste scope alone can't star");
            Assert.That(check!.Status, Is.EqualTo(HttpStatusCode.Forbidden), "paste scope alone can't check");
            Assert.That(checkWithRead, Is.False, "user:read can check");
            Assert.That(toggleWithRead!.Status, Is.EqualTo(HttpStatusCode.Forbidden), "user:read can't star");
            Assert.That(starred, Is.True, "user scope can star");
        });
    }

    [Test]
    public async Task V3Edit_WithoutTitle_StoresAndReturnsEmptyTitle()
    {
        _userContext.LoginUser(new User { Id = "titleless1" }, DefaultScopes);
        var created = await _pasteController.CreatePaste(SimpleCreateInfo(), CancellationToken.None);

        var edited = await _pasteService.EditAsync(created.Id, new PasteEditInfo
        {
            Title = null,
            Pasties = [new PastyEditInfo { Id = created.Pasties[0].Id, Title = "pasty", Content = "x", Language = "Text" }]
        }, CancellationToken.None);
        var stored = await _pasteService.GetAsync(created.Id);

        Assert.Multiple(() =>
        {
            Assert.That(edited.Title, Is.EqualTo(""), "returned");
            Assert.That(stored.Title, Is.EqualTo(""), "stored");
        });
    }

    [Test]
    public async Task V3Create_OnlyBlankTags_IsNotATaggedPaste()
    {
        _userContext.LogoutUser();

        var created = await _pasteService.CreateAsync(new PasteCreateInfo
        {
            Title = "blank tags",
            Tags = ["", "  "],
            Pasties = [new PastyCreateInfo { Title = "", Content = "x", Language = "Text" }]
        }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(created.OwnerId, Is.Null);
            Assert.That(created.Tags, Is.Empty);
        });
    }

    [Test]
    public void V3Create_RealTagWhileLoggedOut_IsStillRejected()
    {
        _userContext.LogoutUser();

        var e = Assert.ThrowsAsync<HttpException>(async () => await _pasteService.CreateAsync(new PasteCreateInfo
        {
            Tags = ["", "real"],
            Pasties = [new PastyCreateInfo { Title = "", Content = "x", Language = "Text" }]
        }, CancellationToken.None));

        Assert.That(e!.Status, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task V2Self_WithOnlyUserReadScope_IncludesStars()
    {
        var user = new User { Id = "selfread1", Username = "selfread1", UserSettings = new UserSettings(), Settings = new Settings() };
        await _fixture.MongoService.Users.InsertOneAsync(user);
        _userContext.LoginUser(user, DefaultScopes);
        var created = await _pasteController.CreatePaste(SimpleCreateInfo(), CancellationToken.None);
        await _pasteService.ToggleStarAsync(created.Id);

        _userContext.LoginUser(user, [Scope.UserRead]);
        var userProvider = new UserProvider(_userContext, _pasteService, _fixture.MongoService, actionLogger: null, imageService: null);
        var self = await new UserControllerV2(userProvider, new ConfigurationBuilder().Build(), _userContext).GetSelf();

        Assert.That(self.Stars, Is.EqualTo(new[] { created.Id }));
    }
}
