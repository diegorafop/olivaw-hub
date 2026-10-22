using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Owh.Olivaw.Api.Extensions.Facades;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest;
public class LeadReadIntegrationTest
{
    [Theory]
    [InlineData("", 401)]
    [InlineData("operator", 403)]
    [InlineData("root", 200)]
    public async Task Read_should_require_root_for_all_routes_without_real_database(string actor, int expected)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", ApplicationName = typeof(WebApplicationBuilderFacadeExtensions).Assembly.FullName });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = "PostgreSql", ["ConnectionStrings:DbContext"] = "Host=localhost;Database=unused",
            ["Identity:Issuer"] = "lead-tests", ["Identity:Audience"] = "lead-tests", ["Identity:SigningKey"] = "test-only-key-with-at-least-32-bytes",
            ["masstransit:configured"] = "false", ["services:loki:configured"] = "false"
        });
        builder.AddSetups();
        builder.Services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "test"; options.DefaultChallengeScheme = "test"; options.DefaultForbidScheme = "test"; }).AddScheme<AuthenticationSchemeOptions, LeadTestAuthentication>("test", _ => { });
        builder.Services.AddAuthorization(options => options.DefaultPolicy = new AuthorizationPolicyBuilder("test").RequireAuthenticatedUser().Build());
        var reader = new Mock<IRepositoryLead>();
        reader.Setup(item => item.ReadAsync(It.IsAny<LeadReadFilter>(), It.IsAny<CancellationToken>())).ReturnsAsync(new LeadReadPage());
        reader.Setup(item => item.ReadSummaryAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(new LeadReadSummary { Total = 2, Recent = 1 });
        builder.Services.AddSingleton(reader.Object);
        var request = LeadFeatureFakerBuilder.Create();
        var lead = Lead.Create(request.ExternalId, new string('a', 64), 1, JsonSerializer.Serialize(request.Payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)), DateTime.UtcNow);
        var repository = new Mock<IRepository<Lead>>();
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>())).Returns<Expression<Func<Lead, bool>>, CancellationToken, Expression<Func<Lead, object>>[]>((filter, _, _) => Task.FromResult(filter.Compile()(lead) ? lead : null!));
        builder.Services.AddSingleton(repository.Object);
        await using var app = builder.Build();
        app.UseSetups();
        await app.StartAsync();
        using var client = app.GetTestClient();
        if (actor.Length > 0) client.DefaultRequestHeaders.Add("X-Test-Actor", actor);
        foreach (var url in new[] { "/leads", "/leads/summary", $"/leads/{request.ExternalId}" })
        {
            using var response = await client.GetAsync(url);
            Assert.Equal((HttpStatusCode)expected, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("writeKeyHash", body);
            Assert.DoesNotContain("actorUserId", body);
            if (expected == 200) Assert.True(response.Headers.CacheControl?.NoStore);
        }
        if (expected != 200)
        {
            reader.Verify(item => item.ReadAsync(It.IsAny<LeadReadFilter>(), It.IsAny<CancellationToken>()), Times.Never);
            repository.Verify(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>()), Times.Never);
        }
        else
        {
            using var invalid = await client.GetAsync("/leads?pageSize=101");
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
            using var missing = await client.GetAsync($"/leads/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        }
    }
}
public sealed class LeadTestAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var actor = Request.Headers["X-Test-Actor"].ToString();
        if (actor.Length == 0) return Task.FromResult(AuthenticateResult.NoResult());
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "test-operator"), new Claim("root", (actor == "root").ToString().ToLowerInvariant()) }, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
