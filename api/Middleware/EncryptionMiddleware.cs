using pastemyst.Services;

namespace pastemyst.Middleware;

public class EncryptionMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext)
    {
        var encryptionContext = httpContext.RequestServices.GetService<EncryptionContext>();

        encryptionContext.EncryptionKey = httpContext.Request.Headers["Encryption-Key"];

        httpContext.Features.Set(encryptionContext);

        await next(httpContext);
    }
}