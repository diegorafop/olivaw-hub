using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Owh.Devops.Api.Extensions.Facades;
using Owh.Devops.Services;
using Xunit;

namespace Owh.Devops.Tests;

public class DiskCleanupIntegrationTest
{
    [Theory]
    [InlineData(true, 202)]
    [InlineData(false, 403)]
    public async Task Cleanup_should_require_root_claim_from_hub(bool root, int status)
    {
        using var cancellation = new System.Threading.CancellationTokenSource();
        var token = cancellation.Token;
        const string key = "test-only-internal-hub-signing-key-at-least-32-bytes";
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing", ApplicationName = typeof(Api.Features.DiskCleanupFeature.DiskCleanupController).Assembly.FullName
        });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = "PostgreSql", ["ConnectionStrings:DbContext"] = "Host=localhost;Database=test",
            ["Identity:Issuer"] = "hub-tests", ["Identity:Audience"] = "hub-tests", ["Identity:SigningKey"] = key,
            ["masstransit:configured"] = "false", ["services:loki:configured"] = "false"
        });
        builder.AddSetups();
        var id = Guid.NewGuid();
        var cleanup = new Mock<IDiskCleanupService>(MockBehavior.Strict);
        if (root) cleanup.Setup(service => service.CreateAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new DeploymentStatus { Id = id, Status = "queued" });
        builder.Services.AddScoped(_ => cleanup.Object);
        await using var app = builder.Build();
        app.UseSetups();
        await app.StartAsync(token);
        using var client = app.GetTestClient();
        var jwt = new JwtSecurityToken("hub-tests", "hub-tests", new[]
        {
            new Claim("sub", Guid.NewGuid().ToString()), new Claim("root", root.ToString().ToLowerInvariant()),
            new Claim("permission", "devops.deploy")
        }, expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(jwt));
        var response = await client.PostAsJsonAsync("/disk-cleanups", new { }, token);
        Assert.Equal((HttpStatusCode)status, response.StatusCode);
        if (root) Assert.EndsWith($"/disk-cleanups/{id}", response.Headers.Location?.ToString());
        cleanup.VerifyAll();
        cleanup.VerifyNoOtherCalls();
    }
}
