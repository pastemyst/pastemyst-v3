using System.Net;
using MongoDB.Driver;
using MongoDB.Bson;
using pastemyst.Exceptions;
using pastemyst.Models;

namespace pastemyst.Services;

public class SettingsService(
    IConfiguration configuration,
    UserProvider userProvider,
    ImageService imageService,
    UserContext userContext,
    IdProvider idProvider,
    MongoService mongo)
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

        return userContext.Self.UserSettings;
    }

    public async Task UpdateUserSettingsAsync(UserSettings settings)
    {
        if (!userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to update user settings.");

        var update = Builders<User>.Update.Set(u => u.UserSettings, settings);
        await mongo.Users.UpdateOneAsync(u => u.Id == userContext.Self.Id, update);
    }

    public async Task<Settings> GetSettingsAsync(HttpContext httpContext)
    {
        // If a logged in user is requesting the settings, send the user's settings
        if (userContext.IsLoggedIn())
        {
            return userContext.Self.Settings;
        }

        var sessionSettingsCookie = httpContext.Request.Cookies["pastemyst_session_settings"];

        // If an anonymous user is requesting the settings, and the session cookie exists, send that
        if (sessionSettingsCookie is not null)
        {
            var sessionSettings = await mongo.SessionSettings.Find(s => s.Id == sessionSettingsCookie).FirstOrDefaultAsync();

            if (sessionSettings is not null)
            {
                // modify the last accessed field
                var update = Builders<SessionSettings>.Update.Set(s => s.LastAccessed, DateTime.UtcNow);
                await mongo.SessionSettings.UpdateOneAsync(s => s.Id == sessionSettingsCookie, update);

                return sessionSettings.Settings;
            }
        }

        // Else, create a new session settings cookie and send that
        var newSessionSettings = new SessionSettings
        {
            Id = await idProvider.GenerateId(async id => await ExistsSessionSettingsByIdAsync(id))
        };

        await mongo.SessionSettings.InsertOneAsync(newSessionSettings);

        var newSessionSettingsCookie = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = configuration.GetValue<bool>("Https"),
            Expires = DateTimeOffset.MaxValue
        };

        httpContext.Response.Cookies.Append("pastemyst_session_settings", newSessionSettings.Id, newSessionSettingsCookie);

        return newSessionSettings.Settings;
    }

    public async Task UpdateSettingsAsync(HttpContext httpContext, Settings settings)
    {
        if (userContext.IsLoggedIn())
        {
            var update = Builders<User>.Update.Set(u => u.Settings, settings);
            await mongo.Users.UpdateOneAsync(u => u.Id == userContext.Self.Id, update);
        }
        else
        {
            var sessionSettingsCookie = httpContext.Request.Cookies["pastemyst_session_settings"];

            if (sessionSettingsCookie is null)
            {
                throw new HttpException(HttpStatusCode.BadRequest, "Can't update the settings since the user is not logged in and the session settings cookie is missing.");
            }

            var sessionSettings = await mongo.SessionSettings.Find(s => s.Id == sessionSettingsCookie).FirstOrDefaultAsync();

            if (sessionSettings is null)
            {
                throw new HttpException(HttpStatusCode.BadRequest, "The session settings cookie has probably expired.");
            }

            var update = Builders<SessionSettings>.Update.Set(s => s.Settings, settings).Set(s => s.LastAccessed, DateTime.UtcNow);
            await mongo.SessionSettings.UpdateOneAsync(s => s.Id == sessionSettings.Id, update);
        }
    }

    private async Task<bool> ExistsSessionSettingsByIdAsync(string id)
    {
        return await mongo.SessionSettings.Find(p => p.Id == id).FirstOrDefaultAsync() is not null;
    }
}
