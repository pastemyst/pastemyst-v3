using System.Net;
using pastemyst.DbContexts;
using pastemyst.Exceptions;

namespace pastemyst.Services;

public interface ISettingsService
{
    public Task SetUsernameAsync(string username);
}

public class SettingsService : ISettingsService
{
    private readonly IAuthService _authService;
    private readonly IUserProvider _userProvider;
    private readonly DataContext _dataContext;
    private readonly IHttpContextAccessor _contextAccessor;

    public SettingsService(IAuthService authService, IHttpContextAccessor contextAccessor, IUserProvider userProvider,
        DataContext dataContext)
    {
        _authService = authService;
        _contextAccessor = contextAccessor;
        _userProvider = userProvider;
        _dataContext = dataContext;
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
}