using pastemyst.Models;
using pastemyst.Models.Auth;

namespace pastemyst.Services;

public class UserContext
{
    public User Self { get; private set; }

    public Scope[] Scopes { get; private set; }

    public bool IsLoggedIn() => Self is not null;

    public bool UserIsSelf(User user) => Self is not null && Self.Id == user.Id;

    public void LoginUser(User user, Scope[] scopes)
    {
        Self = user;
        Scopes = scopes;
    }

    public void LogoutUser() => Self = null;
}
