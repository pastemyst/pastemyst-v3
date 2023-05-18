using System.Net;
using MongoDB.Driver;
using MongoDB.Bson;
using pastemyst.Exceptions;
using pastemyst.Models;

namespace pastemyst.Services;

public interface IUserSettingsService
{
    public Task SetUsernameAsync(string username);

    public Task SetAvatarAsync(byte[] bytes, string contentType);

    public UserSettings GetUserSettings();

    public Task UpdateUserSettingsAsync(UserSettings settings);
}

public class UserSettingsService : IUserSettingsService
{
    private readonly IUserContext _userContext;
    private readonly IUserProvider _userProvider;
    private readonly IImageService _imageService;
    private readonly IMongoService _mongo;

    public UserSettingsService(IUserProvider userProvider, IImageService imageService,
        IUserContext userContext, IMongoService mongo)
    {
        _userProvider = userProvider;
        _imageService = imageService;
        _userContext = userContext;
        _mongo = mongo;
    }

    public async Task SetUsernameAsync(string username)
    {
        if (!_userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You need to be authorized to change settings.");
        }

        if (string.Equals(_userContext.Self.Username, username, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Same username.");
        }

        if (await _userProvider.ExistsByUsernameAsync(username))
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Username already taken.");
        }

        var update = Builders<User>.Update.Set(u => u.Username, username);
        await _mongo.Users.UpdateOneAsync(u => u.Id == _userContext.Self.Id, update);
    }

    public async Task SetAvatarAsync(byte[] bytes, string contentType)
    {
        if (!_userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You need to be authorized to change settings.");
        }

        await _mongo.Images.DeleteAsync(ObjectId.Parse(_userContext.Self.AvatarId));

        var newAvatar = await _imageService.UploadImageAsync(bytes, contentType);

        var update = Builders<User>.Update.Set(u => u.AvatarId, newAvatar);
        await _mongo.Users.UpdateOneAsync(u => u.Id == _userContext.Self.Id, update);
    }

    public UserSettings GetUserSettings()
    {
        if (!_userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to fetch user settings.");

        return _userContext.Self.Settings;
    }

    public async Task UpdateUserSettingsAsync(UserSettings settings)
    {
        if (!_userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to update user settings.");

        var update = Builders<User>.Update.Set(u => u.Settings, settings);
        await _mongo.Users.UpdateOneAsync(u => u.Id == _userContext.Self.Id, update);
    }
}
