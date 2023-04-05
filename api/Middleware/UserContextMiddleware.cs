using pastemyst.Services;

namespace pastemyst.Middleware;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        var self = await authService.GetSelfAsync(context);

        var userContext = context.RequestServices.GetRequiredService<IUserContext>();
        userContext.LoginUser(self);
        
        context.Features.Set(userContext);

        await _next(context);
    }
}