using Owh.OlivawCore.Api.Extensions.Https;
using Owh.OlivawCore.Requests;
using Owh.OlivawCore.Tests.TestSupport;
using System.Security.Claims;
using Xunit;

namespace Owh.OlivawCore.Tests.ExtensionsTest;

public class HttpContextExtensionsTest
{
    [Fact]
    public void GetValue_should_return_first_matching_claim_or_empty()
    {
        Claim[] claims = [new("name", "first"), new("name", "second")];
        Assert.Equal("first", claims.GetValue("name"));
        Assert.Equal(string.Empty, claims.GetValue("missing"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("e30.invalid.invalid")]
    public void JwtTokenIsValid_should_reject_missing_or_malformed_tokens(string? token)
    {
        Assert.False(token!.JwtTokenIsValid());
        Assert.False(((AuthenticateUserCommandResponse?)null).JwtTokenIsValid());
    }

    [Theory]
    [InlineData(-60, 60, true)]
    [InlineData(10, 60, false)]
    [InlineData(-120, -60, false)]
    [InlineData(-10, 0.5, false)]
    public void JwtTokenIsValid_should_enforce_validity_window_with_one_minute_margin(double start, double end, bool expected)
    {
        var now = DateTime.UtcNow;
        var token = TokenFactory.Create(now.AddMinutes(start), now.AddMinutes(end));
        Assert.Equal(expected, token.JwtTokenIsValid());
        Assert.Equal(expected, new AuthenticateUserCommandResponse { Token = token }.JwtTokenIsValid());
    }
}