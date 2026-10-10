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
        // someone else's encrypted paste, starred by the user
        await mongo.EncryptedPastes.InsertOneAsync(new EncryptedPaste { Id = "otherse1", OwnerId = "someone", Stars = [user.Id], EncryptedData = "", Iv = "", Salt = "" });

        var pasteService = new PasteService(new IdProvider(), new LanguageProvider(), _userContext, new EncryptionContext(), new ActionLogger(mongo), mongo);
        var userProvider = new UserProvider(_userContext, pasteService, mongo, new ActionLogger(mongo), new ImageService(mongo));

        _userContext.LoginUser(user, [Scope.User]);
        await userProvider.DeleteUserAsync(user.Username, CancellationToken.None);
        _userContext.LogoutUser();

        var ownPastesLeft = await mongo.BasePastes.CountDocumentsAsync(p => p.OwnerId == user.Id);
        var otherPaste = await mongo.BasePastes.Find(p => p.Id == "otherse1").FirstAsync();

        Assert.Multiple(() =>
        {
            Assert.That(ownPastesLeft, Is.Zero, "unencrypted and encrypted pastes are deleted");
            Assert.That(otherPaste.Stars, Is.Empty, "stars on encrypted pastes are removed");
        });
    }
}
