using pastemyst.Models;

namespace pastemyst.Services;

public class UserContext
{
    public User Self { get; private set;  }

    public bool IsLoggedIn() => Self is not null;

    public bool UserIsSelf(User user) => Self is not null && Self.Id == user.Id;

    public void LoginUser(User user) => Self = user;

    public void LogoutUser() => Self = null;
}
