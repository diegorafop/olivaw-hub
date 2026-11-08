using Microsoft.Extensions.Caching.Memory;
using Owh.OlivawCore.Api.Services;
using Owh.OlivawCore.Api.Settings;
using Owh.OlivawCore.Tests.TestSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Xunit;

namespace Owh.OlivawCore.Tests.ServicesTest;

public class ProviderAuthenticationTest
{
    [Fact]
    public async Task AutenticarAsync_should_send_credentials_once_and_cache_a_valid_token()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var factory = new HttpClientStub();
        var tokenValue = TokenFactory.Create(DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddHours(1));
        var settings = Setting();
        factory.Respond = async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.EndsWith("/authenticate", request.RequestUri!.AbsolutePath);
            var body = JObject.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            Assert.Equal(settings.Login, body["Login"]!.Value<string>());
            Assert.Equal(settings.Password, body["Password"]!.Value<string>());
            return Response(tokenValue);
        };
        var service = new ProviderAuthentication(settings, factory, cache);
        var first = await service.AutenticarAsync(TestContext.Current.CancellationToken);
        var second = await service.AutenticarAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Bearer", first.Scheme);
        Assert.Equal(tokenValue, first.Parameter);
        Assert.Equal(first, second);
        Assert.Equal(1, factory.Requests);
        Assert.Equal("authenticate", service.AutenticarRoute);
    }

    [Fact]
    public async Task AutenticarAsync_should_refresh_an_expired_token()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var factory = new HttpClientStub();
        var expired = TokenFactory.Create(DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1));
        var valid = TokenFactory.Create(DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddHours(1));
        factory.Respond = (request, token) => Task.FromResult(Response(factory.Requests == 1 ? expired : valid));
        var service = new ProviderAuthentication(Setting(), factory, cache);
        var result = await service.AutenticarAsync(TestContext.Current.CancellationToken);
        Assert.Equal(valid, result.Parameter);
        Assert.Equal(2, factory.Requests);
        await service.AutenticarAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, factory.Requests);
    }

    [Fact]
    public void Constructor_should_reject_missing_cache()
    {
        using var factory = new HttpClientStub();
        Assert.Equal("cache", Assert.Throws<ArgumentNullException>(() => new ProviderAuthentication(Setting(), factory, null!)).ParamName);
    }

    private static HttpResponseMessage Response(string token) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(JsonConvert.SerializeObject(new { Token = token }))
    };

    private static SettingProviderAuthentication Setting() => new()
    {
        ServiceName = "auth-test",
        Login = "test-user",
        Password = "dummy-not-a-secret",
        CacheKey = "token",
        CacheSlidingExpiration = 5,
        AuthenticationType = "Bearer",
        Routes = new() { ["autenticar-usuario"] = "authenticate" }
    };
}
