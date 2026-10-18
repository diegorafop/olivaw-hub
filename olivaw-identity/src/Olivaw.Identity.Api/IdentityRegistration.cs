using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Dados;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;

namespace Olivaw.Identity.Api;

public static class IdentityRegistration
{
    public const string Scheme = "OlivawIdentity";

    public static IServiceCollection AddOlivawIdentity<TContext>(this IServiceCollection services, IConfiguration configuration) where TContext : DbContext
    {
        var options = configuration.GetSection("Identity").Get<Security.IdentityOptions>() ?? new();
        if (string.IsNullOrWhiteSpace(options.Issuer)) throw new InvalidOperationException("Configure Identity:Issuer.");
        if (string.IsNullOrWhiteSpace(options.Audience)) throw new InvalidOperationException("Configure Identity:Audience.");
        if (Encoding.UTF8.GetByteCount(options.SigningKey) < 32) throw new InvalidOperationException("Configure Identity:SigningKey com pelo menos 32 bytes.");
        if (options.TokenMinutes < 1) throw new InvalidOperationException("Identity:TokenMinutes deve estar entre 1 e 120.");
        if (options.TokenMinutes > 120) throw new InvalidOperationException("Identity:TokenMinutes deve estar entre 1 e 120.");
        services.AddSingleton(options);
        services.AddScoped<IIdentityStore, IdentityStore<TContext>>();
        services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
        services.AddScoped<IdentityService>();
        services.AddScoped<AuthenticationService>();
        services.AddScoped<RootBootstrap>();
        services.AddScoped<CurrentActor>();
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddMediatR(settings => settings.RegisterServicesFromAssembly(typeof(IdentityRegistration).Assembly));
        services.AddControllers(settings => settings.Filters.Add<IdentityExceptionFilter>())
            .AddApplicationPart(typeof(IdentityRegistration).Assembly);
        services.AddAuthentication(settings =>
        {
            settings.DefaultAuthenticateScheme = Scheme;
            settings.DefaultChallengeScheme = Scheme;
        }).AddJwtBearer(Scheme, settings =>
        {
            settings.MapInboundClaims = false;
            settings.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = true,
                ValidAudience = options.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = "name"
            };
            settings.Events = new JwtBearerEvents { OnTokenValidated = ValidateUserAsync };
        });
        services.AddAuthorization(settings =>
        {
            settings.DefaultPolicy = new AuthorizationPolicyBuilder(Scheme).RequireAuthenticatedUser().Build();
            settings.FallbackPolicy = settings.DefaultPolicy;
            settings.AddPolicy("identity.gerenciar", policy => policy.AddAuthenticationSchemes(Scheme)
                .RequireAuthenticatedUser().AddRequirements(new PermissionRequirement("identity.gerenciar")));
        });
        services.AddRateLimiter(settings =>
        {
            settings.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            settings.AddPolicy("identity-login", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown", item => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
        });
        return services;
    }

    private static async Task ValidateUserAsync(TokenValidatedContext context)
    {
        if (!Guid.TryParse(context.Principal?.FindFirstValue("sub"), out var id))
        {
            context.Fail("Token inválido.");
            return;
        }
        var store = context.HttpContext.RequestServices.GetRequiredService<IIdentityStore>();
        var usuario = await store.FindAsync<Usuario>(item => item.IdExterno == id, context.HttpContext.RequestAborted);
        if (usuario is null)
        {
            context.Fail("Token inválido.");
            return;
        }
        if (!usuario.Ativo)
        {
            context.Fail("Token inválido.");
            return;
        }
        if (context.Principal?.FindFirstValue("stamp") != usuario.VersaoSeguranca.ToString()) context.Fail("Token inválido.");
    }

    public static async Task InitializeOlivawIdentityAsync(this WebApplication application, CancellationToken cancellationToken = default)
    {
        if (!application.Configuration.GetValue<bool>("Identity:Bootstrap:Enabled")) return;
        await using var scope = application.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<RootBootstrap>().EnsureAsync(application.Configuration, cancellationToken);
    }
}
