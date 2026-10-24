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
using Owh.Olivaw.Api.Features.InventoryFeature.Commands;
using Owh.Olivaw.Api.Features.InventoryFeature.Queries;
using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
using Owh.Olivaw.Tests.LeadFeatureTest;
using Xunit;

namespace Owh.Olivaw.Tests.InventoryFeatureTest;
public class InventoryIntegrationTest
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
        mediator.Setup(item => item.Send(It.IsAny<ReadInventoryResourceByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadInventoryResourceByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<ReadInventoryResourceByIdMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadInventoryResourceByIdResponse());
        mediator.Setup(item => item.Send(It.IsAny<ReadServiceDeploymentByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadServiceDeploymentByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<ReadServiceDeploymentByIdMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadServiceDeploymentByIdResponse());
        mediator.Setup(item => item.Send(It.IsAny<ReadInventoryLinkByFilterMediatorQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadInventoryLinkByFilterResponse());
        mediator.Setup(item => item.Send(It.IsAny<CreateInventoryResourceMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateInventoryResourceResult());
        mediator.Setup(item => item.Send(It.IsAny<UpdateInventoryResourceMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UpdateInventoryResourceResult());
        mediator.Setup(item => item.Send(It.IsAny<CreateServiceDeploymentMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateServiceDeploymentResult());
        mediator.Setup(item => item.Send(It.IsAny<UpdateServiceDeploymentMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UpdateServiceDeploymentResult());
        mediator.Setup(item => item.Send(It.IsAny<CreateResourceCustomerMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateResourceCustomerResult());
        mediator.Setup(item => item.Send(It.IsAny<UpdateResourceCustomerMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UpdateResourceCustomerResult());
        mediator.Setup(item => item.Send(It.IsAny<CreateResourceDependencyMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateResourceDependencyResult());
        mediator.Setup(item => item.Send(It.IsAny<UpdateResourceDependencyMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UpdateResourceDependencyResult());
        mediator.Setup(item => item.Send(It.IsAny<CreateDeploymentResourceMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateDeploymentResourceResult());
        mediator.Setup(item => item.Send(It.IsAny<UpdateDeploymentResourceMediatorCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UpdateDeploymentResourceResult());
        builder.Services.AddSingleton(mediator.Object);
        await using var app = builder.Build(); app.UseSetups(); await app.StartAsync();
        using var client = app.GetTestClient();
        if (actor.Length > 0) client.DefaultRequestHeaders.Add("X-Test-Actor", actor);
        foreach (var url in new[] { "/resources?search=&status=&type=&provider=&environment=", "/resources/00000000-0000-0000-0000-000000000020", "/deployments?environment=", "/deployments/00000000-0000-0000-0000-000000000020", "/inventory-links?resourceExternalId=00000000-0000-0000-0000-000000000020" })
        {
            using var response = await client.GetAsync(url);
            Assert.Equal((HttpStatusCode)expected, response.StatusCode);
            if (expected == 200) Assert.True(response.Headers.CacheControl?.NoStore);
        }
        foreach (var route in new[] { "/resources", "/deployments", "/resource-customers", "/resource-dependencies", "/deployment-resources" })
        {
            using var created = await client.PostAsJsonAsync(route, new { });
            Assert.Equal(expected == 200 ? HttpStatusCode.Created : (HttpStatusCode)expected, created.StatusCode);
            using var updated = await client.PutAsJsonAsync(route + "/00000000-0000-0000-0000-000000000020", new { });
            Assert.Equal((HttpStatusCode)expected, updated.StatusCode);
        }
    }
}
