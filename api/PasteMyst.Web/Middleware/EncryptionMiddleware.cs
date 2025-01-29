using PasteMyst.Web.Services;

namespace PasteMyst.Web.Middleware;

public class EncryptionMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext)
    {
        var encryptionContext = httpContext.RequestServices.GetService<EncryptionContext>();

        encryptionContext.EncryptionKey = httpContext.Request.Headers["Encryption-Key"];

        foreach (var cookie in httpContext.Request.Cookies)
        {
            if (!cookie.Key.StartsWith("pastemyst-encryption-key-")) continue;

            var paste = cookie.Key["pastemyst-encryption-key-".Length..];
            encryptionContext.EncryptionKeys[paste] = cookie.Value;
        }

        httpContext.Features.Set(encryptionContext);

        await next(httpContext);
    }
}