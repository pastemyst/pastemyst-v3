using System.Security.Claims;
using OpenIddict.Abstractions;

namespace pastemyst.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool IsLoggedIn(this ClaimsPrincipal principal) => principal?.Identity?.IsAuthenticated ?? false;
    
    public static string Id(this ClaimsPrincipal principal) => principal?.GetClaim(ClaimTypes.NameIdentifier);
}