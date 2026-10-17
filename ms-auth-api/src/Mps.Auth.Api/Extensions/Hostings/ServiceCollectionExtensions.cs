using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Mps.Auth.Api.Extensions.Hostings;
using Mps.Auth.Api.Services;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Extensions.Hostings;
using Mps.HolosCore.Api.Extensions.Https;
using Mps.HolosCore.Api.Validation;
using System.Diagnostics.CodeAnalysis;

namespace Mps.Auth.Api.Extensions.Hostings;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddSettingConfiguration
    (
        this IServiceCollection services,
        ConfigurationManager configuration
    )
    {
        services.AddSettingConfigurationCore(configuration);

        services.AddSingleton(item => configuration.GetSettingUsuario());
        services.AddSingleton(item => configuration.GetSettingDocumento());
        services.AddSingleton(item => configuration.GetSettingNotificacao());
    }

    public static void AddServiceUsuario
    (
        this IServiceCollection services,
        ConfigurationManager configuration
    )
    {
        var setting = configuration.GetSettingUsuario();

        services.AddHttpClient
        (
            setting.ServiceName,
            setting.GetHttpClientFactory()
        );

        services.AddScoped<IServiceUsuario, ServiceUsuario>();
    }

    public static void AddServiceDocumento
    (
        this IServiceCollection services,
        ConfigurationManager configuration
    )
    {
        var setting = configuration.GetSettingDocumento();

        services.AddHttpClient
        (
            setting.ServiceName,
            setting.GetHttpClientFactory()
        );

        services.AddScoped<IServiceDocumento, ServiceDocumento>();
    }

    public static void AddServiceNotificacao
    (
        this IServiceCollection services,
        ConfigurationManager configuration
    )
    {
        var setting = configuration.GetSettingNotificacao();

        services.AddHttpClient
        (
            setting.ServiceName,
            setting.GetHttpClientFactory()
        );

        services.AddScoped<IServiceNotificacao, ServiceNotificacao>();
    }

    public static void AddAuthentication
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
                if (configuration.GetKeyCloakProviderConfigured())
                    options.Authority = configuration.GetValue<string>("keycloak-provider:authority");
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = configuration.GetTokenValidationParameters();
            });
    }

    public static void AddAuthenticationAd
    (
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddAuthentication()
            .AddOpenIdConnect(options =>
            {
                options.Authority = configuration.GetValueByKey<string>("keycloak-provider:authority");
                options.ClientId = configuration.GetValueByKey<string>("keycloak-provider:clientId");
                options.ClientSecret = configuration.GetValueByKey<string>("keycloak-provider:clientSecret");
                options.ResponseType = configuration.GetValueByKey<string>("keycloak-provider:responseType");

                var itens = configuration.GetManyScope();

                foreach (string scope in itens)
                    options
                    .Scope
                    .Add(scope);
            });
    }

    public static void AddFluentValidation
    (
        this IServiceCollection services
    )
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FluentValidatorBehavior<,>));
    }
}