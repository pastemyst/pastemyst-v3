using System.Net;
using System.Security.Claims;
using MongoDB.Driver;
using MongoDB.Bson;
using pastemyst.Exceptions;
using pastemyst.Extensions;
using pastemyst.Models;

namespace pastemyst.Services;

public class SettingsService(
    IConfiguration configuration,
    UserProvider userProvider,
    ImageService imageService,
    IdProvider idProvider,
    MongoService mongo)
{
    public async Task SetUsernameAsync(ClaimsPrincipal self, string username)
    {
        if (!self.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You need to be authorized to change settings.");
        }
        
        var selfUser = await userProvider.GetSelfAsync(self);

        if (string.Equals(selfUser.Username, username, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Same username.");
        }

        if (await userProvider.ExistsByUsernameAsync(username))
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Username already taken.");
        }

        var update = Builders<User>.Update.Set(u => u.Username, username);
        await mongo.Users.UpdateOneAsync(u => u.Id == selfUser.Id, update);
    }

    public async Task SetAvatarAsync(ClaimsPrincipal self, byte[] bytes, string contentType)
    {
        if (!self.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You need to be authorized to change settings.");
        }
        
        var selfUser = await userProvider.GetSelfAsync(self);

        await mongo.Images.DeleteAsync(ObjectId.Parse(selfUser.AvatarId));

        var newAvatar = await imageService.UploadImageAsync(bytes, contentType);

        var update = Builders<User>.Update.Set(u => u.AvatarId, newAvatar);
        await mongo.Users.UpdateOneAsync(u => u.Id == selfUser.Id, update);
    }

    public async Task<UserSettings> GetUserSettingsAsync(ClaimsPrincipal self)
    {
        if (!self.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to fetch user settings.");
        
        var selfUser = await userProvider.GetSelfAsync(self);

        return selfUser.UserSettings;
    }

    public async Task UpdateUserSettingsAsync(ClaimsPrincipal self, UserSettings settings)
    {
        if (!self.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to update user settings.");
        
        var selfUser = await userProvider.GetSelfAsync(self);

        var update = Builders<User>.Update.Set(u => u.UserSettings, settings);
        await mongo.Users.UpdateOneAsync(u => u.Id == selfUser.Id, update);
    }

    public async Task<Settings> GetSettingsAsync(HttpContext httpContext, ClaimsPrincipal self)
    {
        // If a logged in user is requesting the settings, send the user's settings
        if (self.IsLoggedIn())
        {
            var selfUser = await userProvider.GetSelfAsync(self);
            return selfUser.Settings;
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

    public async Task UpdateSettingsAsync(HttpContext httpContext, ClaimsPrincipal self, Settings settings)
    {
        if (self.IsLoggedIn())
        {
            var selfUser = await userProvider.GetSelfAsync(self);
            var update = Builders<User>.Update.Set(u => u.Settings, settings);
            await mongo.Users.UpdateOneAsync(u => u.Id == selfUser.Id, update);
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
