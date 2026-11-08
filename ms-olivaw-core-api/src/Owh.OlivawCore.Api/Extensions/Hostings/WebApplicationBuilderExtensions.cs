using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Owh.OlivawCore.Api.Extensions.Entities;
using Owh.OlivawCore.Api.Extensions.Hostings;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using System.Reflection;

namespace Owh.OlivawCore.Api.Extensions.Hostings;

public static class WebApplicationBuilderExtensions
{
    public static void AddSecurityCore
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddAuthenticationCore(builder.Configuration);

        builder
            .Services
            .AddAuthorization();

        builder
            .Services
            .AddCorsDefaultPolicyCore();
    }

    public static void AddControllerCore
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

    public static void AddSwagger
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddSwagger(builder.Configuration);
    }

    public static void AddMassTransit
    (
        this WebApplicationBuilder builder,
        Assembly assembly
    )
    {
        builder
            .Services
            .AddMassTransit
            (
                builder.Configuration,
                assembly
            );
    }

    public static void AddSettingConfigurationCore
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddSettingConfigurationCore(builder.Configuration);
    }

    public static void AddServiceExternalCore
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddProviderAuthentication(builder.Configuration);
    }

    public static void AddMemoryCacheService
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddMemoryCacheService();
    }

    public static void AddServiceResponseValidator
    (
        this WebApplicationBuilder builder
    )
    {
        builder
            .Services
            .AddServiceResponseValidator();
    }

    public static void AddServiceLogEngine
    (
        this WebApplicationBuilder builder
    )
    {
        var setting = builder
            .Configuration
            .GetSettingLoki();

        if (!setting.Configured)
            return;

        builder
            .Logging
            .ClearProviders();

        builder
            .Services
            .AddServiceLoki(builder);

        builder
            .Logging
            .AddSerilog(Log.Logger);

        builder
            .Host
            .UseSerilog();
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
}