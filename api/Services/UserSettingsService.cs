using System.Net;
using pastemyst.DbContexts;
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
    private readonly DataContext _dataContext;

    public UserSettingsService(DataContext dataContext, IUserProvider userProvider, IImageService imageService,
        IUserContext userContext)
    {
        _dataContext = dataContext;
        _userProvider = userProvider;
        _imageService = imageService;
        _userContext = userContext;
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

        _userContext.Self.Username = username;

        _dataContext.Users.Attach(_userContext.Self);
        _dataContext.Users.Entry(_userContext.Self).Property(u => u.Username).IsModified = true;
        await _dataContext.SaveChangesAsync();
    }

    public async Task SetAvatarAsync(byte[] bytes, string contentType)
    {
        if (!_userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You need to be authorized to change settings.");
        }

        await _imageService.DeleteAsync(_userContext.Self.Avatar);

        var newAvatar = await _imageService.UploadImageAsync(bytes, contentType);

        _userContext.Self.Avatar = newAvatar;

        _dataContext.Users.Attach(_userContext.Self);
        _dataContext.Users.Entry(_userContext.Self).Reference(u => u.Avatar).IsModified = true;
        await _dataContext.SaveChangesAsync();
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

        _userContext.Self.Settings = settings;

        _dataContext.Users.Attach(_userContext.Self);
        _dataContext.Users.Entry(_userContext.Self).Reference(u => u.Settings).IsModified = true;
        await _dataContext.SaveChangesAsync();
    }
}