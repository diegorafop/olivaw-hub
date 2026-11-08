using Owh.OlivawCore.Api.Extensions.Https;
using Xunit;

namespace Owh.OlivawCore.Tests.ExtensionsTest;

public class HttpClientExtensionsTest
{
    [Theory]
    [InlineData(null, "/items")]
    [InlineData("mailto:service@example.test", "/items")]
    [InlineData("https://service.test/", "/items")]
    [InlineData("https://service.test/api/", "items")]
    public void GetRoute_should_strip_leading_slash_only_when_base_address_has_a_path(string? baseAddress, string expected)
    {
        using var client = new HttpClient();
        if (baseAddress is not null)
            client.BaseAddress = new Uri(baseAddress);
        Assert.Equal(expected, client.GetRoute("/items"));
        Assert.Equal("/items", ((HttpClient)null!).GetRoute("/items"));
    }
}