using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Owh.Devops.Api.Extensions.Facades;
using Owh.Devops.Dados;
using Olivaw.Identity.Domain;
using Authorization = Olivaw.Identity.Domain.Authorization;
using Xunit;

namespace Owh.Devops.Tests;

public class IdentityIntegrationTest
{
    [Fact]
    public async Task Host_should_use_hub_without_local_identity_tables_or_services()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing",
            ApplicationName = typeof(Api.Extensions.Facades.WebApplicationBuilderFacadeExtensions).Assembly.FullName
        });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = "PostgreSql",
            ["ConnectionStrings:DbContext"] = "Host=localhost;Database=template_tests;SearchPath=public",
            ["Identity:Issuer"] = "template-tests", ["Identity:Audience"] = "template-tests",
            ["Identity:SigningKey"] = "test-only-key-with-at-least-32-bytes",
            ["masstransit:configured"] = "false", ["services:loki:configured"] = "false",
            ["services:provider-authentication:serviceName"] = "unused"
        });
        builder.AddSetups();
        await using var app = builder.Build();
        app.UseSetups();
        using var cancellation = new CancellationTokenSource();
        await app.StartAsync(cancellation.Token);
        using var client = app.GetTestClient();
        var unauthenticated = await client.GetAsync("/identity/userprofiles", cancellation.Token);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);
        var invalidLogin = await client.PostAsJsonAsync("/identity/authentication/login", new { }, cancellation.Token);
        Assert.Equal(HttpStatusCode.Unauthorized, invalidLogin.StatusCode);
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        Assert.Null(context.Model.FindEntityType(typeof(UserProfile)));
        Assert.Null(context.Model.FindEntityType(typeof(Authorization)));
        Assert.Null(context.Model.FindEntityType(typeof(AccessProfile)));
        Assert.Null(context.Model.FindEntityType(typeof(UserProfileAccessProfile)));
        Assert.Null(context.Model.FindEntityType(typeof(AccessProfileAuthorization)));
        Assert.Empty(context.Model.GetEntityTypes());
        Assert.Null(scope.ServiceProvider.GetService<Olivaw.Identity.Repositories.IIdentityStore>());
        Assert.Null(scope.ServiceProvider.GetService<Olivaw.Identity.Api.Services.AuthenticationService>());
        Assert.Null(scope.ServiceProvider.GetService<Olivaw.Identity.Api.Services.RootBootstrap>());
        var actions = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>();
        Assert.DoesNotContain(actions.ActionDescriptors.Items, action => action.AttributeRouteInfo?.Template?.StartsWith("identity/") == true);
    }
}
