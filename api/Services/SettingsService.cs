using System.Net;
using pastemyst.DbContexts;
using pastemyst.Exceptions;

namespace pastemyst.Services;

public interface ISettingsService
{
    public Task SetUsernameAsync(string username);

    public Task SetAvatarAsync(byte[] bytes, string contentType);
}

public class SettingsService : ISettingsService
{
    private readonly IAuthService _authService;
    private readonly IUserProvider _userProvider;
    private readonly IImageService _imageService;
    private readonly DataContext _dataContext;
    private readonly IHttpContextAccessor _contextAccessor;

    public SettingsService(IAuthService authService, IHttpContextAccessor contextAccessor, IUserProvider userProvider,
        DataContext dataContext, IImageService imageService)
    {
        _authService = authService;
        _contextAccessor = contextAccessor;
        _userProvider = userProvider;
        _dataContext = dataContext;
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
}