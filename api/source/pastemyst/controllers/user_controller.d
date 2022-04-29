module pastemyst.controllers.user_controller;

import pastemyst.models;
import pastemyst.services;
import vibe.d;

/**
 * API /api/v3/user
 */
@path("/api/v3/user")
public interface IUserController
{
    /**
     * GET /api/v3/user/:username
     *
     * Returns the user with the provided username.
     */
    @path("/:username")
    const(User) getUser(string _username) @safe;
}

/**
 * API /api/v3/user
 */
public class UserController : IUserController
{
    private UserService userService;

    ///
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
