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
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;
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
    [InlineData("admin", 200)]
    [InlineData("company", 403)]
    [InlineData("revoked", 403)]
    public async Task Read_should_require_root_or_lead_permission_for_all_routes_without_real_database(string actor, int expected)
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
        var identityStore = new Mock<IIdentityStore>();
        var user = new UserProfile { ExternalId = Guid.Parse("00000000-0000-0000-0000-000000000017"), IsRoot = actor == "root", SecurityStamp = Guid.Parse("00000000-0000-0000-0000-000000000001") };
        identityStore.Setup(item => item.FindAsync(It.IsAny<Expression<Func<UserProfile, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        IReadOnlyList<string> permissions = Array.Empty<string>();
        if (actor == "admin") permissions = new[] { "leads.manage" };
        identityStore.Setup(item => item.PermissionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(permissions);
        builder.Services.AddSingleton(identityStore.Object);
        var reader = new Mock<IRepositoryLead>();
        reader.Setup(item => item.ReadAsync(It.IsAny<LeadReadFilter>(), It.IsAny<CancellationToken>())).ReturnsAsync(new LeadReadPage());
        reader.Setup(item => item.ReadSummaryAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(new LeadReadSummary { Total = 2, Recent = 1 });
        builder.Services.AddSingleton(reader.Object);
        var request = LeadFeatureFakerBuilder.Create();
        var lead = Lead.Create(request.ExternalId, new string('a', 64), 1, JsonSerializer.Serialize(request.Payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)), DateTime.UtcNow);
        var repository = new Mock<IRepository<Lead>>();
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>())).Returns<Expression<Func<Lead, bool>>, CancellationToken, Expression<Func<Lead, object>>[]>((filter, _, _) => Task.FromResult(filter.Compile()(lead) ? lead : null!));
        repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(lead);
        builder.Services.AddSingleton(repository.Object);
        var changes = new Mock<IRepository<LeadChange>>();
        changes.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<LeadChange, bool>>>(), 21, It.IsAny<Expression<Func<LeadChange, object>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<LeadChange, object>>[]>())).ReturnsAsync(Array.Empty<LeadChange>());
        builder.Services.AddSingleton(changes.Object);
        await using var app = builder.Build();
        app.UseSetups();
        await app.StartAsync();
        using var client = app.GetTestClient();
        if (actor.Length > 0) client.DefaultRequestHeaders.Add("X-Test-Actor", actor);
        foreach (var url in new[] { "/leads", "/leads/summary", $"/leads/{request.ExternalId}", $"/leads/{request.ExternalId}/history" })
        {
            using var response = await client.GetAsync(url);
            Assert.Equal((HttpStatusCode)expected, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("writeKeyHash", body);
            Assert.DoesNotContain("actorUserId", body);
            if (expected == 200) Assert.True(response.Headers.CacheControl?.NoStore);
        }
        using var create = await client.PostAsJsonAsync("/leads", new
        {
            externalId = Guid.NewGuid(), initiative = "outbound", origin = "99Freelas",
            title = "Comentários", requirements = "Integrar recursos", sourceUrl = "", externalReference = "795293"
        });
        Assert.Equal((HttpStatusCode)(expected == 200 ? 201 : expected), create.StatusCode);
        using var proposal = await client.PutAsJsonAsync($"/leads/{request.ExternalId}/proposal", new
        {
            revision = 1, proposal = new
            {
                title = "Comentários", rows = new[] { new { description = "Diagnóstico", role = "Analista", solution = "analysis", hours = 4, hourlyRate = 100 } }
            }
        });
        Assert.Equal((HttpStatusCode)expected, proposal.StatusCode);
        if (expected == 200)
        {
            using var proposalRetry = await client.PutAsJsonAsync($"/leads/{request.ExternalId}/proposal", new
            {
                revision = 1, proposal = new { title = "Comentários", rows = new[] { new { description = "Diagnóstico", role = "Analista", solution = "analysis", hours = 4, hourlyRate = 100 } } }
            });
            Assert.Equal(HttpStatusCode.Conflict, proposalRetry.StatusCode);
        }
        using var update = await client.PutAsJsonAsync($"/leads/{request.ExternalId}/follow-up", new { revision = 0, status = "analysis", note = "Nota" });
        Assert.Equal((HttpStatusCode)expected, update.StatusCode);
        if (expected != 200)
        {
            reader.Verify(item => item.ReadAsync(It.IsAny<LeadReadFilter>(), It.IsAny<CancellationToken>()), Times.Never);
            repository.Verify(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>()), Times.Never);
        }
        else
        {
            using var retry = await client.PutAsJsonAsync($"/leads/{request.ExternalId}/follow-up", new { revision = 0, status = "analysis", note = "Nota" });
            Assert.Equal(HttpStatusCode.Conflict, retry.StatusCode);
            using var invalidStatus = await client.PutAsJsonAsync($"/leads/{request.ExternalId}/follow-up", new { revision = 1, status = "invalid", note = "Nota" });
            Assert.Equal(HttpStatusCode.BadRequest, invalidStatus.StatusCode);
            using var invalidCursor = await client.GetAsync($"/leads/{request.ExternalId}/history?beforeRevision=0");
            Assert.Equal(HttpStatusCode.BadRequest, invalidCursor.StatusCode);
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
        var identity = new ClaimsIdentity(new[] { new Claim("sub", "00000000-0000-0000-0000-000000000017"), new Claim("stamp", "00000000-0000-0000-0000-000000000001"), new Claim("permission", actor is "admin" or "revoked" ? "leads.manage" : "portal.read"), new Claim("root", (actor == "root").ToString().ToLowerInvariant()) }, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
