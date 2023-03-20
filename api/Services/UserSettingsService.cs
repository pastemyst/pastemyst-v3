using System.Net;
using pastemyst.DbContexts;
using pastemyst.Exceptions;
using pastemyst.Models;

namespace pastemyst.Services;

public interface IUserSettingsService
{
    public Task SetUsernameAsync(string username);

    public Task SetAvatarAsync(byte[] bytes, string contentType);

    public Task<UserSettings> GetUserSettingsAsync();

    public Task UpdateUserSettingsAsync(UserSettings settings);
}

public class UserSettingsService : IUserSettingsService
{
    private readonly IAuthService _authService;
    private readonly IUserProvider _userProvider;
    private readonly IImageService _imageService;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly DataContext _dataContext;

    public UserSettingsService(DataContext dataContext, IAuthService authService, IHttpContextAccessor contextAccessor,
        IUserProvider userProvider, IImageService imageService)
    {
        _dataContext = dataContext;
        _authService = authService;
        _contextAccessor = contextAccessor;
        _userProvider = userProvider;
        _imageService = imageService;
    }

    public async Task SetUsernameAsync(string username)
    {
        var self = await _authService.GetSelfAsync(_contextAccessor.HttpContext);

        if (self is null)
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You need to be authorized to change settings.");
        }

        if (string.Equals(self.Username, username, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Same username.");
        }

        if (await _userProvider.ExistsByUsernameAsync(username))
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Username already taken.");
        }

        self.Username = username;

        _dataContext.Users.Attach(self);
        _dataContext.Users.Entry(self).Property(u => u.Username).IsModified = true;
        await _dataContext.SaveChangesAsync();
    }

    public async Task SetAvatarAsync(byte[] bytes, string contentType)
    {
        var self = await _authService.GetSelfAsync(_contextAccessor.HttpContext);

        if (self is null)
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You need to be authorized to change settings.");
        }

        await _imageService.DeleteAsync(self.Avatar);

        var newAvatar = await _imageService.UploadImageAsync(bytes, contentType);

        self.Avatar = newAvatar;

        _dataContext.Users.Attach(self);
        _dataContext.Users.Entry(self).Reference(u => u.Avatar).IsModified = true;
        await _dataContext.SaveChangesAsync();
    }

    public async Task<UserSettings> GetUserSettingsAsync()
    {
        var self = await _authService.GetSelfAsync(_contextAccessor.HttpContext);

        if (self is null)
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to fetch user settings.");

        return self.Settings;
    }

    public async Task UpdateUserSettingsAsync(UserSettings settings)
    {
        var self = await _authService.GetSelfAsync(_contextAccessor.HttpContext);

        if (self is null)
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to update user settings.");

        self.Settings = settings;
        
        _dataContext.Users.Attach(self);
        _dataContext.Users.Entry(self).Reference(u => u.Settings).IsModified = true;
        await _dataContext.SaveChangesAsync();
    }
}