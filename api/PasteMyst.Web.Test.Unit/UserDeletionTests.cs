using MongoDB.Driver;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Test.Unit;

public sealed class UserDeletionTests
{
    private readonly DatabaseFixture _fixture = new();
    private readonly UserContext _userContext = new();

    [OneTimeTearDown]
    public void OneTimeTearDown() => _fixture.Dispose();

    [Test]
    public async Task DeleteUser_RemovesEncryptedPastesAndStarsOnThem()
    {
        var mongo = _fixture.MongoService;
        var avatarId = await mongo.Images.UploadFromBytesAsync("avatar", [1, 2, 3]);
        var user = new User
        {
            Id = "leaver01", Username = "leaver01", AvatarId = avatarId.ToString(),
            UserSettings = new UserSettings(), Settings = new Settings()
        };
        await mongo.Users.InsertOneAsync(user);

        await mongo.Pastes.InsertOneAsync(new Paste { Id = "leaverp1", OwnerId = user.Id, Pasties = [] });
        await mongo.EncryptedPastes.InsertOneAsync(new EncryptedPaste { Id = "leavere1", OwnerId = user.Id, EncryptedData = "", Iv = "", Salt = "" });
        await mongo.AccessTokens.InsertOneAsync(new AccessToken { Id = "leavtok1", Token = "x", OwnerId = user.Id, Scopes = [Scope.Paste] });
        await mongo.AccessTokens.InsertOneAsync(new AccessToken { Id = "keeptok1", Token = "y", OwnerId = "someone", Scopes = [Scope.Paste] });
        // someone else's encrypted paste, starred by the user
        await mongo.EncryptedPastes.InsertOneAsync(new EncryptedPaste { Id = "otherse1", OwnerId = "someone", Stars = [user.Id], EncryptedData = "", Iv = "", Salt = "" });

        var pasteService = new PasteService(new IdProvider(), new LanguageProvider(), _userContext, new EncryptionContext(), new ActionLogger(mongo), mongo);
        var userProvider = new UserProvider(_userContext, pasteService, mongo, new ActionLogger(mongo), new ImageService(mongo));

        _userContext.LoginUser(user, [Scope.User]);
        await userProvider.DeleteUserAsync(user.Username, CancellationToken.None);
        _userContext.LogoutUser();

        var ownPastesLeft = await mongo.BasePastes.CountDocumentsAsync(p => p.OwnerId == user.Id);
        var otherPaste = await mongo.BasePastes.Find(p => p.Id == "otherse1").FirstAsync();
        var ownTokensLeft = await mongo.AccessTokens.CountDocumentsAsync(t => t.OwnerId == user.Id);
        var otherTokenKept = await mongo.AccessTokens.Find(t => t.Id == "keeptok1").AnyAsync();

        Assert.Multiple(() =>
        {
            Assert.That(ownPastesLeft, Is.Zero, "unencrypted and encrypted pastes are deleted");
            Assert.That(otherPaste.Stars, Is.Empty, "stars on encrypted pastes are removed");
            Assert.That(ownTokensLeft, Is.Zero, "the user's access tokens are deleted");
            Assert.That(otherTokenKept, Is.True, "other users' tokens stay");
        });
    }

    [Test]
    public async Task UnknownUsername_Returns404_ForTagsExportAndDelete()
    {
        var mongo = _fixture.MongoService;
        var pasteService = new PasteService(new IdProvider(), new LanguageProvider(), _userContext, new EncryptionContext(), new ActionLogger(mongo), mongo);
        var userProvider = new UserProvider(_userContext, pasteService, mongo, new ActionLogger(mongo), new ImageService(mongo));

        var admin = new User { Id = "admin404", Username = "admin404", IsAdmin = true, UserSettings = new UserSettings(), Settings = new Settings() };
        _userContext.LoginUser(admin, [Scope.User, Scope.Paste]);

        var tags = Assert.ThrowsAsync<PasteMyst.Web.Exceptions.HttpException>(async () => await userProvider.GetTagsAsync("nobody-at-all", CancellationToken.None));
        var export = Assert.ThrowsAsync<PasteMyst.Web.Exceptions.HttpException>(async () => await userProvider.DownloadUserData("nobody-at-all", CancellationToken.None));
        var delete = Assert.ThrowsAsync<PasteMyst.Web.Exceptions.HttpException>(async () => await userProvider.DeleteUserAsync("nobody-at-all", CancellationToken.None));
        _userContext.LogoutUser();

        Assert.Multiple(() =>
        {
            Assert.That(tags!.Status, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
            Assert.That(export!.Status, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
            Assert.That(delete!.Status, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
        });
    }
}
