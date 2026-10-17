using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Mps.OlivawCore.Api.Extensions.Hostings;
using Mps.OlivawCore.Api.Extensions.Https;
using Mps.OlivawCore.Api.Services;
using Mps.OlivawCore.Api.Validation;
using Mps.OlivawCore.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using System.Reflection;
using System.Text;

namespace Mps.OlivawCore.Api.Extensions.Hostings;

public static class ServiceCollectionExtensions
{
    public static void AddMemoryCacheService
    (
        this IServiceCollection services
    )
    {
        services.AddMemoryCache();
    }

    public static void AddServiceResponseValidator
    (
        this IServiceCollection services
    )
    {
        services.AddScoped<ServiceResponseValidator>();
    }

    public static void AddProviderAuthentication
    (
        this IServiceCollection services,
        ConfigurationManager configuration
    )
    {
        var setting = configuration.GetSettingProviderAuthentication();

        services.AddHttpClient
        (
            setting.ServiceName,
            setting.GetHttpClientFactory()
        );

        services.AddScoped<IProviderAuthentication, ProviderAuthentication>();
    }

    public static void AddSettingConfigurationCore
    (
        this IServiceCollection services,
        ConfigurationManager configuration
    )
    {
        services.AddSingleton(item => configuration.GetSettingProviderAuthentication());
        services.AddSingleton(item => configuration.GetSettingMassTransit());
        services.AddSingleton(item => configuration.GetSettingLoki());
    }

    public static void AddSwagger
    (
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = configuration["swagger:title"] ?? "OlivawCore Api";
                document.Info.Version = configuration["swagger:version"] ?? "v1";
                return Task.CompletedTask;
            });
        });
    }

    public static void UseSwaggerDocumentation
    (
        this IEndpointRouteBuilder app
    )
    {
        app.MapOpenApi();
    }

    public static void AddAuthenticationCore
    (
        this IServiceCollection services,
        ConfigurationManager configuration
    )
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = configuration.GetTokenValidationParameters();
            });
    }

    public static void AddControllersDefault
    (
        this IServiceCollection services
    )
    {
        services.AddControllers(options =>
        {
            options
                .Filters
                .Add(new FluentValidationExceptionFilter());

            options
                .Filters
                .Add
                (
                    new ProducesResponseTypeAttribute
                    (
                        typeof(ProblemDetails),
                        StatusCodes.Status500InternalServerError
                    )
                );

            options.OutputFormatters.RemoveType<StringOutputFormatter>();
        })
        .AddNewtonsoftJson(options =>
        {
            DefaultContractResolver contractResolver = new()
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = false
                }
            };

            options.SerializerSettings.ContractResolver = contractResolver;
            options.SerializerSettings.Formatting = Formatting.Indented;
        });
    }

    public static void AddMediatRCore
    (
        this IServiceCollection services,
        Assembly assembly
    )
    {
        services.AddMediatR(item =>
        {
            item.RegisterServicesFromAssembly(assembly);
        });
    }

    public static void AddCorsDefaultPolicyCore
    (
        this IServiceCollection services
    )
    {
        services
            .AddCors(options =>
            {
                options
                    .AddDefaultPolicy(builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });
    }

    public static TokenValidationParameters GetTokenValidationParameters
    (
        this ConfigurationManager configuration
    )
    {
        return new TokenValidationParameters
        {
            ValidIssuer = configuration["authentication:issuer"],
            ValidAudience = configuration["authentication:audience"],
            IssuerSigningKey = GetSymmetricSecurityKey(configuration),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true
        };
    }

    private static SymmetricSecurityKey GetSymmetricSecurityKey
    (
        ConfigurationManager configuration
    )
    {
        return new SymmetricSecurityKey(GetSecret(configuration));
    }

    private static byte[] GetSecret
    (
        ConfigurationManager configuration
    )
    {
        var secret = configuration.GetValue<string>("authentication:secret");

        if (string.IsNullOrEmpty(secret))
            return [];

        return Encoding
            .UTF8
            .GetBytes(secret);
    }

    public static void AddServiceLoki
    (
        this IServiceCollection services,
        WebApplicationBuilder builder
    )
    {
        var setting = builder
            .Configuration
            .GetSettingLoki();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom
            .Configuration(builder.Configuration)
            .Enrich
            .FromLogContext()
            .WriteTo
            .Console()
            .WriteTo
            .GrafanaLoki
            (
                setting.BaseAddress,
                labels: [
                    builder.GetLokiLabelByServiceName(),
                    builder.GetLokiLabelByEnvironment()
                ]
            )
            .CreateLogger();

        services.AddHttpLogging(setting);
    }

    public static void AddHttpLogging
    (
        this IServiceCollection services,
        ISettingLoki setting
    )
    {
        services.AddHttpLogging(item =>
        {
            item.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod
                | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath
                | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestBody
                | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode
                | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseBody;

            item.MediaTypeOptions.AddText("application/json");

            item.RequestBodyLogLimit = setting.GetBodyLogLimit("request-body-log-limit");
            item.ResponseBodyLogLimit = setting.GetBodyLogLimit("response-body-log-limit");
        });
    }

    private static int GetBodyLogLimit
    (
        this ISettingLoki? setting,
        string key
    )
    {
        const int valueDefault = 4096;

        if (setting is null)
            return valueDefault;

        if (setting.Claims is null)
            return valueDefault;

        return GetClaimByKey
        (
            setting,
            valueDefault,
            key
        );
    }

    private static int GetClaimByKey
    (
        ISettingLoki setting,
        int valueDefault,
        string key
    )
    {
        var value = setting.Claims[key];

        if (string.IsNullOrEmpty(value))
            return valueDefault;

        return Convert.ToInt32(value);
    }
}