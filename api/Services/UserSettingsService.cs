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

public class UserSettingsService(
    IUserProvider userProvider,
    IImageService imageService,
    IUserContext userContext,
    IMongoService mongo)
    : IUserSettingsService
{
    public async Task SetUsernameAsync(string username)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You need to be authorized to change settings.");
        }

        if (string.Equals(userContext.Self.Username, username, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Same username.");
        }

        if (await userProvider.ExistsByUsernameAsync(username))
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Username already taken.");
        }

        var update = Builders<User>.Update.Set(u => u.Username, username);
        await mongo.Users.UpdateOneAsync(u => u.Id == userContext.Self.Id, update);
    }

    public async Task SetAvatarAsync(byte[] bytes, string contentType)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You need to be authorized to change settings.");
        }

        await mongo.Images.DeleteAsync(ObjectId.Parse(userContext.Self.AvatarId));

        var newAvatar = await imageService.UploadImageAsync(bytes, contentType);

        var update = Builders<User>.Update.Set(u => u.AvatarId, newAvatar);
        await mongo.Users.UpdateOneAsync(u => u.Id == userContext.Self.Id, update);
    }

    public UserSettings GetUserSettings()
    {
        if (!userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to fetch user settings.");

        return userContext.Self.Settings;
    }

    public async Task UpdateUserSettingsAsync(UserSettings settings)
    {
        if (!userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to update user settings.");

        var update = Builders<User>.Update.Set(u => u.Settings, settings);
        await mongo.Users.UpdateOneAsync(u => u.Id == userContext.Self.Id, update);
    }
}
