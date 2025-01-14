using pastemyst.Services;

namespace pastemyst.Middleware;

public class UserContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AuthService authService)
    {
        var (self, scopes) = await authService.GetSelfWithScopesAsync(context);

        var userContext = context.RequestServices.GetRequiredService<UserContext>();
        userContext.LoginUser(self, scopes);

        context.Features.Set(userContext);

        await next(context);
    }
}
