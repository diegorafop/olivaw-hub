using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Owh.Olivaw.Api.Services;
using Xunit;
namespace Owh.Olivaw.Tests.HealthReadingFeatureTest;
public sealed class PublicHealthProbeTest
{
    [Theory]
    [InlineData("127.0.0.1", false)] [InlineData("10.0.0.1", false)] [InlineData("169.254.169.254", false)]
    [InlineData("172.16.0.1", false)] [InlineData("192.168.1.1", false)] [InlineData("100.64.0.1", false)]
    [InlineData("::1", false)] [InlineData("::ffff:127.0.0.1", false)] [InlineData("8.8.8.8", true)]
    public void Policy_should_reject_private_and_metadata_destinations(string ip, bool expected) => Assert.Equal(expected, PublicHealthProbe.IsPublicIPv4(IPAddress.Parse(ip)));
    [Theory]
    [InlineData(200, "success")] [InlineData(503, "failure")] [InlineData(302, "failure")]
    public async Task Read_should_use_exact_authorization_and_preserve_http_result(int status, string outcome)
    {
        using var client = new HttpClient(new ResponseHandler((HttpStatusCode)status));
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Monitoring:AllowedUrls:0"] = "https://example.test/ready" }).Build();
        var probe = new PublicHealthProbe(client, config);
        Assert.True(probe.IsAllowed("https://example.test/ready"));
        Assert.False(probe.IsAllowed("https://example.test/other")); Assert.False(probe.IsAllowed("http://example.test/ready"));
        Assert.False(probe.IsAllowed("https://example.test/ready?token=test")); Assert.False(probe.IsAllowed("https://user:pass@example.test/ready"));
        Assert.Equal(outcome, await probe.ReadAsync("https://example.test/ready", CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() => probe.ReadAsync("https://example.test/other", CancellationToken.None));
        using var transport = PublicHealthProbe.CreateHandler(); Assert.False(transport.AllowAutoRedirect); Assert.False(transport.UseProxy); Assert.False(transport.UseCookies);
    }
    private sealed class ResponseHandler(HttpStatusCode status) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => Task.FromResult(new HttpResponseMessage(status));
    }
}
