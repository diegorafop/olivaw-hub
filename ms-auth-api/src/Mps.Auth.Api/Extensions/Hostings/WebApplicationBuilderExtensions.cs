using Microsoft.EntityFrameworkCore;
using Mps.Auth.Dados;
using Mps.Auth.Dados.Extensions.Facades;
using Mps.HolosCore.Api.Extensions.Hostings;
using Serilog.Sinks.Grafana.Loki;
using System.Diagnostics.CodeAnalysis;

namespace Mps.Auth.Api.Extensions.Hostings;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderExtensions
{
    public static void AddSecurity
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddAuthentication(builder.Configuration);

        var keyCloakProviderConfigured = builder
            .Configuration
            .GetKeyCloakProviderConfigured();

        if (keyCloakProviderConfigured)
            builder
                .Services
                .AddAuthenticationAd(builder.Configuration);

        builder
            .Services
            .AddAuthorization();

        builder
            .Services
            .AddCorsDefaultPolicyCore();
    }

    public static void AddControllers
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddHttpContextAccessor();

        builder
            .Services
            .AddEndpointsApiExplorer();

        builder
            .Services
            .AddHealthChecks();

        builder
            .Services
            .AddControllersDefault();
    }

    public static void AddFluentValidation
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddFluentValidation();
    }

    public static void AddMediatR
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddMediatRCore(typeof(Program).Assembly);
    }

    public static void AddSettingConfiguration
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddSettingConfiguration(builder.Configuration);
    }

    public static void AddServiceExternal
    (
        this WebApplicationBuilder builder
    )
    {
        builder.AddServiceExternalCore();

        builder
            .Services
            .AddServiceUsuario(builder.Configuration);

        builder
            .Services
            .AddServiceNotificacao(builder.Configuration);

        builder
            .Services
            .AddServiceDocumento(builder.Configuration);
    }

    public static LokiLabel GetLokiLabelByServiceName
    (
        this WebApplicationBuilder builder
    )
    {
        var serviceName = builder
            .Configuration
            .GetServiceNameByTitle();

        return new LokiLabel
        {
            Key = "service_name",
            Value = serviceName
        };
    }

    public static LokiLabel GetLokiLabelByEnvironment
    (
        this WebApplicationBuilder builder
    )
    {
        var environmentName = builder
            .Configuration
            .GetEnvironmentByTitle();

        return new LokiLabel
        {
            Key = "environment",
            Value = environmentName
        };
    }

    public static string GetServiceName
    (
        this WebApplicationBuilder builder
    )
    {
        var environment = builder.Environment;

        if (environment is null)
            return "unknown";

        var name = environment.EnvironmentName;

        if (string.IsNullOrEmpty(name))
            return "unknown";

        return name.ToLower();
    }

    public static string GetEnvironmentName
    (
        this WebApplicationBuilder builder
    )
    {
        var environment = builder.Environment;

        if (environment is null)
            return "unknown";

        var name = environment.EnvironmentName;

        if (string.IsNullOrEmpty(name))
            return "unknown";

        return name.ToLower();
    }

    public static void AddDatabase
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddDatabase
            (
                builder.Configuration,
                builder
                    .Environment
                    .IsProduction()
            );
    }

    public static void UseMigrations
    (
        this WebApplication webApplication
    )
    {
        using var scope = webApplication
            .Services
            .CreateScope();

        var dataBase = scope
            .ServiceProvider
            .GetRequiredService<WriteDbContext>();

        dataBase
            .Database
            .Migrate();
    }
}