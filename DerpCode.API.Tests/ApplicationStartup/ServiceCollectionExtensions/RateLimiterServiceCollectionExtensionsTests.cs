using System.Net;
using System.Security.Claims;
using DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Http;

namespace DerpCode.API.Tests.ApplicationStartup.ServiceCollectionExtensions;

public sealed class RateLimiterServiceCollectionExtensionsTests
{
    #region GetIpAddress Tests

    [Fact]
    public void GetIpAddress_WithRemoteIpAddress_ReturnsRemoteIpAddressString()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");

        // Act
        var result = context.GetIpAddress();

        // Assert
        Assert.Equal("203.0.113.7", result);
    }

    [Fact]
    public void GetIpAddress_WithIpv6RemoteIpAddress_ReturnsIpv6String()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("2001:db8::1");

        // Act
        var result = context.GetIpAddress();

        // Assert
        Assert.Equal("2001:db8::1", result);
    }

    [Fact]
    public void GetIpAddress_IgnoresRawXForwardedForHeader()
    {
        // Arrange — represents an attacker spoofing X-Forwarded-For directly to the API. The
        // ForwardedHeaders middleware would normally only apply this header when the immediate
        // connection comes from a trusted proxy; the rate limiter must not bypass that check by
        // reading the raw header itself.
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");
        context.Request.Headers["X-Forwarded-For"] = "1.1.1.1, 2.2.2.2";

        // Act
        var result = context.GetIpAddress();

        // Assert
        Assert.Equal("203.0.113.7", result);
    }

    [Fact]
    public void GetIpAddress_WithNullRemoteIpAddress_ReturnsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = null;

        // Act
        var result = context.GetIpAddress();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetIpAddress_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((HttpContext)null!).GetIpAddress());
    }

    #endregion

    #region GetPartitionKey Tests

    [Fact]
    public void GetPartitionKey_WithAuthenticatedUser_ReturnsUserName()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "alice")],
            authenticationType: "TestAuth");
        context.User = new ClaimsPrincipal(identity);

        // Act
        var result = context.GetPartitionKey();

        // Assert — authenticated identity wins over IP so a single user can't dodge throttles by
        // moving networks mid-attack, and shared NATs (e.g., corporate egress) aren't punished for
        // a single noisy account.
        Assert.Equal("alice", result);
    }

    [Fact]
    public void GetPartitionKey_WithAnonymousUserAndIp_ReturnsIpAddress()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");
        context.Request.Headers["X-Forwarded-For"] = "9.9.9.9";

        // Act
        var result = context.GetPartitionKey();

        // Assert
        Assert.Equal("203.0.113.7", result);
    }

    [Fact]
    public void GetPartitionKey_WithAnonymousUserAndNoIp_ReturnsAnonymous()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = null;

        // Act
        var result = context.GetPartitionKey();

        // Assert
        Assert.Equal("anonymous", result);
    }

    [Fact]
    public void GetPartitionKey_WithUnnamedAuthenticatedIdentity_FallsBackToIp()
    {
        // Arrange — identity authenticated but Name is null (e.g., a JWT without `name` claim).
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(authenticationType: "TestAuth"));

        // Act
        var result = context.GetPartitionKey();

        // Assert
        Assert.Equal("203.0.113.7", result);
    }

    [Fact]
    public void GetPartitionKey_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((HttpContext)null!).GetPartitionKey());
    }

    #endregion
}
