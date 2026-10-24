using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Owh.Olivaw.Api.Extensions.Facades;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Commands;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Queries;
using Owh.Olivaw.Client.Features.ManagedServiceFeature.Queries;
using Owh.Olivaw.Tests.LeadFeatureTest;
using Xunit;

namespace Owh.Olivaw.Tests.ManagedServiceFeatureTest;
public class ManagedServiceIntegrationTest
{
    [Theory]
    [InlineData("", 401)]
    [InlineData("company", 403)]
    [InlineData("admin", 403)]
    [InlineData("root", 200)]
    public async Task Routes_should_require_root_and_never_access_real_database(string actor, int expected)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", ApplicationName = typeof(WebApplicationBuilderFacadeExtensions).Assembly.FullName });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["DatabaseProvider"] = "PostgreSql", ["ConnectionStrings:DbContext"] = "Host=localhost;Database=unused", ["Identity:Issuer"] = "test", ["Identity:Audience"] = "test", ["Identity:SigningKey"] = "test-only-key-with-at-least-32-bytes", ["masstransit:configured"] = "false", ["services:loki:configured"] = "false" });
        builder.AddSetups();
        builder.Services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "test"; options.DefaultChallengeScheme = "test"; options.DefaultForbidScheme = "test"; }).AddScheme<AuthenticationSchemeOptions, LeadTestAuthentication>("test", _ => { });
        builder.Services.AddAuthorization(options => options.DefaultPolicy = new AuthorizationPolicyBuilder("test").RequireAuthenticatedUser().Build());
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.IsAny<ReadManagedServiceByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadManagedServiceByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<ReadManagedServiceByIdMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadManagedServiceByIdResponse());
        mediator.Setup(item => item.Send(It.IsAny<CreateManagedServiceMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateManagedServiceResult { Response = new() { ExternalId = Guid.NewGuid() } });
        mediator.Setup(item => item.Send(It.IsAny<UpdateManagedServiceMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UpdateManagedServiceResult());
        builder.Services.AddSingleton(mediator.Object);
        await using var app = builder.Build(); app.UseSetups(); await app.StartAsync();
        using var client = app.GetTestClient();
        if (actor.Length > 0) client.DefaultRequestHeaders.Add("X-Test-Actor", actor);
        foreach (var url in new[] { "/services", "/services/00000000-0000-0000-0000-000000000018" })
        {
            using var response = await client.GetAsync(url);
            Assert.Equal((HttpStatusCode)expected, response.StatusCode);
            if (expected == 200) Assert.True(response.Headers.CacheControl?.NoStore);
        }
        using var created = await client.PostAsJsonAsync("/services", new { });
        Assert.Equal(expected == 200 ? HttpStatusCode.Created : (HttpStatusCode)expected, created.StatusCode);
        using var updated = await client.PutAsJsonAsync("/services/00000000-0000-0000-0000-000000000018", new { });
        Assert.Equal((HttpStatusCode)expected, updated.StatusCode);
    }
}
