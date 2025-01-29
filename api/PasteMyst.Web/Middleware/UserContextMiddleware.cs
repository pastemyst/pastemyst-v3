using PasteMyst.Web.Services;

namespace PasteMyst.Web.Middleware;

public sealed class UserContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AuthService authService)
    {
        var token = context?.RequestAborted ?? CancellationToken.None;
        var (self, scopes) = await authService.GetSelfWithScopesAsync(context, token);

        var userContext = context.RequestServices.GetRequiredService<UserContext>();
        userContext.LoginUser(self, scopes);

        context.Features.Set(userContext);

        await next(context);
    }
}
