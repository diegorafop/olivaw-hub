using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Mps.OlivawCore.Api.Extensions.Hostings;
using Mps.OlivawCore.Api.Services;
using Mps.OlivawCore.Api.Settings;
using Mps.OlivawCore.Api.Validation;
using Mps.OlivawCore.Services;
using Mps.OlivawCore.Settings;
using Newtonsoft.Json;
using Xunit;

namespace Mps.OlivawCore.Tests.ExtensionsTest;

public class ServiceCollectionExtensionsTest
{
    [Fact]
    public void Registrations_should_resolve_settings_cache_and_scoped_authentication_client()
    {
        using var configuration = Configuration();
        var services = new ServiceCollection();
        services.AddSettingConfigurationCore(configuration);
        services.AddMemoryCacheService();
        services.AddProviderAuthentication(configuration);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var auth = firstScope.ServiceProvider.GetRequiredService<IProviderAuthentication>();
        Assert.IsType<ProviderAuthentication>(auth);
        Assert.Same(auth, firstScope.ServiceProvider.GetRequiredService<IProviderAuthentication>());
        Assert.NotSame(auth, secondScope.ServiceProvider.GetRequiredService<IProviderAuthentication>());
        Assert.Same(provider.GetRequiredService<ISettingProviderAuthentication>(), firstScope.ServiceProvider.GetRequiredService<ISettingProviderAuthentication>());
        Assert.IsType<SettingMassTransit>(provider.GetRequiredService<SettingMassTransit>());
        Assert.IsType<MemoryCache>(provider.GetRequiredService<IMemoryCache>());
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("auth");
        Assert.Equal(new Uri("https://service.test/api/"), client.BaseAddress);
        Assert.Equal(TimeSpan.FromSeconds(20), client.Timeout);
        Assert.Contains(client.DefaultRequestHeaders.Accept, item => item.MediaType == "application/json");
    }

    [Fact]
    public void AddControllersDefault_should_register_validation_filter_and_camel_case_json()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllersDefault();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<MvcOptions>>().Value;
        Assert.Contains(options.Filters, item => item is ServiceFilterAttribute filter && filter.ServiceType == typeof(FluentValidationExceptionFilter));
        Assert.IsType<FluentValidationExceptionFilter>(provider.GetRequiredService<FluentValidationExceptionFilter>());
        Assert.DoesNotContain(options.OutputFormatters, item => item is StringOutputFormatter);
        var json = provider.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value.SerializerSettings;
        var result = JsonConvert.SerializeObject(new { DisplayName = "OlivawCore" }, json);
        Assert.Contains("\"displayName\"", result);
    }

    [Fact]
    public void AddCorsDefault_should_register_the_existing_default_policy()
    {
        var services = new ServiceCollection();
        services.AddCorsDefaultPolicyCore();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CorsOptions>>().Value;
        var policy = options.GetPolicy(options.DefaultPolicyName)!;
        Assert.True(policy.AllowAnyOrigin);
        Assert.True(policy.AllowAnyHeader);
        Assert.True(policy.AllowAnyMethod);
    }

    [Fact]
    public void AddAuthentication_should_register_bearer_scheme_and_configured_signing_key()
    {
        using var configuration = Configuration();
        configuration["authentication:secret"] = Guid.NewGuid().ToString("N");
        configuration["authentication:issuer"] = "test-issuer";
        configuration["authentication:audience"] = "test-audience";
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthenticationCore(configuration);
        using var provider = services.BuildServiceProvider();
        var schemes = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        var bearer = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get("Bearer");
        Assert.Equal("Bearer", schemes.DefaultScheme);
        Assert.Equal("test-issuer", bearer.TokenValidationParameters.ValidIssuer);
        Assert.Equal("test-audience", bearer.TokenValidationParameters.ValidAudience);
        Assert.True(bearer.TokenValidationParameters.ValidateIssuerSigningKey);
        Assert.NotNull(bearer.TokenValidationParameters.IssuerSigningKey);
    }

    [Fact]
    public void Builder_extensions_should_register_components_without_starting_a_host()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        using var config = Configuration();
        builder.Configuration.AddConfiguration(config);
        builder.AddSettingConfigurationCore();
        builder.AddMemoryCacheService();
        builder.AddServiceExternalCore();
        builder.AddMassTransit(typeof(Program).Assembly);
        builder.AddSwagger();
        using var app = builder.Build();
        Assert.IsType<MemoryCache>(app.Services.GetRequiredService<IMemoryCache>());
        using var scope = app.Services.CreateScope();
        Assert.IsType<ProviderAuthentication>(scope.ServiceProvider.GetRequiredService<IProviderAuthentication>());
        Assert.IsType<NullPublishEndpoint>(scope.ServiceProvider.GetRequiredService<MassTransit.IPublishEndpoint>());
    }

    private static ConfigurationManager Configuration()
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["services:provider-authentication:ServiceName"] = "auth",
            ["services:provider-authentication:BaseAddress"] = "https://service.test/api/",
            ["services:provider-authentication:HttpClientTimeout"] = "20",
            ["masstransit:Configured"] = "false"
        });
        return configuration;
    }
}
