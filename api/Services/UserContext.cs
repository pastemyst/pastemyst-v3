using pastemyst.Models;

namespace pastemyst.Services;

public interface IUserContext
{
    public User Self { get; }

    public bool IsLoggedIn();
    public bool UserIsSelf(User user);
    public void LoginUser(User user);
    public void LogoutUser();
}

public class UserContext : IUserContext
{
    public User Self { get; private set;  }

    public bool IsLoggedIn() => Self is not null;

    public bool UserIsSelf(User user) => Self is not null && Self.Id == user.Id;

    public void LoginUser(User user) => Self = user;

    public void LogoutUser() => Self = null;
}