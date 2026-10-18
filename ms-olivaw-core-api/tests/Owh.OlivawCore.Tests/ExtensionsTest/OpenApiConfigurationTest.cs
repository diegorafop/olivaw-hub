using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Owh.OlivawCore.Api.Extensions.Hostings;
using Xunit;

namespace Owh.OlivawCore.Tests.ExtensionsTest;

public class OpenApiConfigurationTest
{
    [Theory]
    [InlineData(null, null, "OlivawCore Api", "v1")]
    [InlineData("Shared API", "v2", "Shared API", "v2")]
    public async Task AddSwagger_should_apply_configured_or_default_document_metadata(
        string? title, string? version, string expectedTitle, string expectedVersion)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Configuration["swagger:title"] = title;
        builder.Configuration["swagger:version"] = version;
        builder.AddSwagger();
        await using var app = builder.Build();
        app.UseSwaggerDocumentation();
        using var scope = app.Services.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredKeyedService<IOpenApiDocumentProvider>("v1");
        var document = await provider.GetOpenApiDocumentAsync(TestContext.Current.CancellationToken);
        Assert.Equal(expectedTitle, document.Info.Title);
        Assert.Equal(expectedVersion, document.Info.Version);
        Assert.Contains(((IEndpointRouteBuilder)app).DataSources.SelectMany(item => item.Endpoints),
            item => item is RouteEndpoint route && route.RoutePattern.RawText == "/openapi/{documentName}.json");
    }

    [Fact]
    public void AddAuthentication_should_fail_when_signing_configuration_is_missing()
    {
        using var configuration = new ConfigurationManager();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthenticationCore(configuration);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
        Assert.Throws<ArgumentException>(() => options.Get("Bearer"));
    }
}