using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;
using Owh.Olivaw.Api.Extensions.Facades;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.LeadFeatureTest;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;
namespace Owh.Olivaw.Tests.SecurityReportFeatureTest;
public sealed class SecurityReportHttpTest
{
    [Theory]
    [InlineData("", 401, 401)]
    [InlineData("operator", 403, 403)]
    [InlineData("writer", 403, 201)]
    [InlineData("root", 200, 201)]
    public async Task Routes_should_enforce_real_authorization_policies_without_database(string actor, int readStatus, int writeStatus)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", ApplicationName = typeof(WebApplicationBuilderFacadeExtensions).Assembly.FullName });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = "PostgreSql", ["ConnectionStrings:DbContext"] = "Host=localhost;Database=unused",
            ["Identity:Issuer"] = "security-tests", ["Identity:Audience"] = "security-tests", ["Identity:SigningKey"] = "test-only-key-with-at-least-32-bytes",
            ["masstransit:configured"] = "false", ["services:loki:configured"] = "false"
        });
        builder.AddSetups();
        builder.Services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "test"; options.DefaultChallengeScheme = "test"; options.DefaultForbidScheme = "test"; }).AddScheme<AuthenticationSchemeOptions, LeadTestAuthentication>("test", _ => { });
        builder.Services.AddAuthorization(options => options.DefaultPolicy = new AuthorizationPolicyBuilder("test").RequireAuthenticatedUser().Build());
        var store = new Mock<IIdentityStore>();
        var user = new UserProfile { ExternalId = Guid.Parse("00000000-0000-0000-0000-000000000017"), IsRoot = actor == "root", SecurityStamp = Guid.Parse("00000000-0000-0000-0000-000000000001") };
        store.Setup(item => item.FindAsync(It.IsAny<Expression<Func<UserProfile, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        IReadOnlyList<string> permissions = actor == "writer" ? new[] { "security.reports.write" } : Array.Empty<string>();
        store.Setup(item => item.PermissionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(permissions);
        builder.Services.AddSingleton(store.Object);
        var repository = new Mock<IRepository<SecurityReport>>();
        repository.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<SecurityReport, bool>>>(), 21, It.IsAny<Expression<Func<SecurityReport, object>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<SecurityReport, object>>[]>())).ReturnsAsync(Array.Empty<SecurityReport>());
        builder.Services.AddSingleton(repository.Object);
        await using var app = builder.Build();
        app.UseSetups(); await app.StartAsync();
        using var client = app.GetTestClient();
        if (actor.Length > 0) client.DefaultRequestHeaders.Add("X-Test-Actor", actor);
        using var read = await client.GetAsync("/security-reports");
        Assert.Equal((HttpStatusCode)readStatus, read.StatusCode);
        using var write = await client.PostAsJsonAsync("/security-reports", SecurityReportFakerBuilder.Create());
        Assert.Equal((HttpStatusCode)writeStatus, write.StatusCode);
        using var detail = await client.GetAsync("/security-reports/00000000-0000-0000-0000-000000000123");
        var expectedDetail = readStatus == 200 ? 404 : readStatus;
        Assert.Equal((HttpStatusCode)expectedDetail, detail.StatusCode);
        if (writeStatus != 201) repository.Verify(item => item.AddAsync(It.IsAny<SecurityReport>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
