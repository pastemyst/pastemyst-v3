using System.Net;
using PasteMyst.Web.Extensions;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace PasteMyst.Web.Test.Unit;

public sealed class RateLimitingTests
{
    private static readonly IPNetwork[] Exempt = [IPNetwork.Parse("192.168.0.0/24")];
    private static readonly IPNetwork[] TrustedProxies = [IPNetwork.Parse("172.16.0.0/12")];

    [TestCase("203.0.113.7", "203.0.113.7")]
    [TestCase("::ffff:203.0.113.7", "203.0.113.7")]        // IPv4-mapped IPv6 is keyed as the IPv4 address
    [TestCase("2001:db8::1", "2001:db8::1")]
    [TestCase("127.0.0.1", null)]                           // loopback is never limited
    [TestCase("::1", null)]
    [TestCase("192.168.0.42", null)]                        // exempt network
    [TestCase("192.168.1.42", "192.168.1.42")]              // just outside it
    [TestCase("172.18.0.3", null)]                          // a proxy that didn't pass on a client IP
    public void GetClientKey(string ip, string? expected)
    {
        Assert.That(RateLimitingExtensions.GetClientKey(IPAddress.Parse(ip), Exempt, TrustedProxies), Is.EqualTo(expected));
    }

    [Test]
    public void GetClientKey_NoIp_IsNotLimited()
    {
        Assert.That(RateLimitingExtensions.GetClientKey(null, Exempt, TrustedProxies), Is.Null);
    }
}
