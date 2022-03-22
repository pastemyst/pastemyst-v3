module pastemyst.controllers.user;

import vibe.d;
import pastemyst.models;
import pastemyst.services;

@path("/api/v3/user")
public interface IUserController
{
    @path("/:username")
    const(User) getUser(string _username) @safe;
}

public class UserController : IUserController
{
    private UserService userService;

    public this(UserService userService)
    {
        this.userService = userService;
    }

    public override const(User) getUser(string username) @safe
    {
        const user = userService.findByUsername(username);

        if (user.isNull()) throw new HTTPStatusException(HTTPStatus.notFound);

        return user.get();
    }
}
