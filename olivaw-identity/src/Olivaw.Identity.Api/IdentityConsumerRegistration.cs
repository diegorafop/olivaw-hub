using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Olivaw.Identity.Api.Security;

namespace Olivaw.Identity.Api;

public static partial class IdentityRegistration
{
    public static IServiceCollection AddOlivawIdentityConsumer(this IServiceCollection services, IConfiguration configuration)
    {
        var options = ReadOptions(configuration);
        services.AddControllers(settings => settings.Filters.Add<IdentityExceptionFilter>())
            .ConfigureApplicationPartManager(manager =>
            {
                var identityName = typeof(IdentityRegistration).Assembly.GetName().Name;
                var parts = manager.ApplicationParts.Where(part => part.Name == identityName).ToArray();
                foreach (var part in parts) manager.ApplicationParts.Remove(part);
            });
        services.AddSingleton(options);
        services.AddScoped<JwtIdentitySession>();
        services.AddScoped<CurrentActor>();
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthorizationHandler, JwtPermissionHandler>();
        services.AddAuthentication(settings =>
        {
            settings.DefaultAuthenticateScheme = Scheme;
            settings.DefaultChallengeScheme = Scheme;
        }).AddJwtBearer(Scheme, settings =>
        {
            settings.MapInboundClaims = false;
            settings.TokenValidationParameters = TokenParameters(options);
            settings.Events = new JwtBearerEvents { OnTokenValidated = ValidateSubjectAsync };
        });
        services.AddAuthorization(settings =>
        {
            settings.DefaultPolicy = new AuthorizationPolicyBuilder(Scheme).RequireAuthenticatedUser().Build();
            settings.FallbackPolicy = settings.DefaultPolicy;
        });
        services.AddRateLimiter(_ => { });
        return services;
    }

    private static Task ValidateSubjectAsync(TokenValidatedContext context)
    {
        if (!Guid.TryParse(context.Principal?.FindFirstValue("sub"), out var id))
        {
            context.Fail("Token inválido.");
            return Task.CompletedTask;
        }
        if (id == Guid.Empty) context.Fail("Token inválido.");
        return Task.CompletedTask;
    }
}
