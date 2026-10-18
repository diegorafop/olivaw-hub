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
using Mps.Ltmj.Api.Extensions.Facades;
using Mps.Ltmj.Dados;
using Olivaw.Identity.Domain;
using Xunit;

namespace Mps.Ltmj.Tests;

public class IdentityIntegrationTest
{
    [Fact]
    public async Task Host_should_import_identity_routes_security_and_model_without_external_auth_service()
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
        var unauthenticated = await client.GetAsync("/identity/usuarios", cancellation.Token);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);
        var invalidLogin = await client.PostAsJsonAsync("/identity/autenticacao/login", new { }, cancellation.Token);
        Assert.Equal(HttpStatusCode.BadRequest, invalidLogin.StatusCode);
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        Assert.NotNull(context.Model.FindEntityType(typeof(Usuario)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Autorizacao)));
        Assert.NotNull(context.Model.FindEntityType(typeof(PerfilAcesso)));
        Assert.NotNull(context.Model.FindEntityType(typeof(UsuarioPerfilAcesso)));
        Assert.NotNull(context.Model.FindEntityType(typeof(PerfilAcessoAutorizacao)));
    }
}
