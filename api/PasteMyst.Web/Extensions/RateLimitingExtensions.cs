using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.V2;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace PasteMyst.Web.Extensions;

public class TokenBucketSettings
{
    public int TokenLimit { get; set; }
    public int TokensPerPeriod { get; set; }
    public int ReplenishmentPeriodSeconds { get; set; }
}

public class RateLimitingSettings
{
    public bool Enabled { get; set; } = true;

    /// <summary>Applies to every request.</summary>
    public TokenBucketSettings General { get; set; } = new() { TokenLimit = 100, TokensPerPeriod = 30, ReplenishmentPeriodSeconds = 1 };

    /// <summary>Applies on top of <see cref="General"/> to endpoints with the paste-create policy.</summary>
    public TokenBucketSettings PasteCreate { get; set; } = new() { TokenLimit = 5, TokensPerPeriod = 1, ReplenishmentPeriodSeconds = 60 };

    /// <summary>Networks allowed to set X-Forwarded-For (nginx via Docker, the client container). Loopback is always trusted.</summary>
    public List<string> TrustedProxies { get; set; } = [];

    /// <summary>Client networks that are never rate limited. Loopback is always exempt.</summary>
    public List<string> ExemptNetworks { get; set; } = [];
}

/// <summary>
/// Per client IP rate limiting. The API sits behind nginx and is also called server-side by the
/// client container, so the client IP comes from X-Forwarded-For, which is only trusted from
/// <see cref="RateLimitingSettings.TrustedProxies"/>.
/// </summary>
public static class RateLimitingExtensions
{
    public const string PasteCreatePolicy = "paste-create";

    private const string RejectionMessage = "Too many requests, please slow down.";

    public static IServiceCollection AddPasteMystRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("RateLimiting").Get<RateLimitingSettings>() ?? new RateLimitingSettings();
        var trustedProxies = settings.TrustedProxies.Select(n => IPNetwork.Parse(n)).ToList();
        var exemptNetworks = settings.ExemptNetworks.Select(n => IPNetwork.Parse(n)).ToList();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
            foreach (var network in trustedProxies) options.KnownNetworks.Add(network);
        });

        services.AddRateLimiter(options =>
        {
            options.OnRejected = OnRejectedAsync;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context => Partition(context, settings.General));
            options.AddPolicy(PasteCreatePolicy, context => Partition(context, settings.PasteCreate));
        });

        return services;

        RateLimitPartition<string> Partition(HttpContext context, TokenBucketSettings bucket)
        {
            var key = settings.Enabled ? GetClientKey(context.Connection.RemoteIpAddress, exemptNetworks, trustedProxies) : null;

            if (key is null) return RateLimitPartition.GetNoLimiter("");

            return RateLimitPartition.GetTokenBucketLimiter(key, _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = bucket.TokenLimit,
                TokensPerPeriod = bucket.TokensPerPeriod,
                ReplenishmentPeriod = TimeSpan.FromSeconds(bucket.ReplenishmentPeriodSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            });
        }
    }

    /// <summary>
    /// The rate limiting key for a client IP (after X-Forwarded-For processing), or null if it isn't rate limited.
    /// </summary>
    public static string? GetClientKey(IPAddress? ip, IReadOnlyList<IPNetwork> exemptNetworks, IReadOnlyList<IPNetwork> trustedProxies)
    {
        if (ip is null) return null;
        if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();

        if (IPAddress.IsLoopback(ip) || exemptNetworks.Any(n => n.Contains(ip))) return null;

        // Still a proxy address after X-Forwarded-For processing means our own infrastructure called
        // without passing on a client IP. Limiting that would put every such request in one bucket.
        if (trustedProxies.Any(n => n.Contains(ip))) return null;

        return ip.ToString();
    }

    private static async ValueTask OnRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

        if (context.HttpContext.Request.Path.StartsWithSegments("/api/v2"))
            await response.WriteAsJsonAsync(new ErrorResponseV2(RejectionMessage), cancellationToken);
        else
            await response.WriteAsJsonAsync(new ErrorResponse((int)HttpStatusCode.TooManyRequests, RejectionMessage), cancellationToken);
    }
}
