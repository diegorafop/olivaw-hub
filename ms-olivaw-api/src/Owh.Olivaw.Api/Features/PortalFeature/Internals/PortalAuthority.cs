using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Dados;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;
using Owh.Olivaw.Dados;
namespace Owh.Olivaw.Api.Features.PortalFeature.Internals;
public sealed class PortalAuthorityOptions
{
    public bool Configured { get; init; }
    public global::Olivaw.Identity.Api.Security.IdentityOptions Identity { get; init; } = new();
}
public sealed class PortalAuthentication([FromKeyedServices("portal")] IIdentityStore store, IPasswordHasher<UserProfile> hasher, PortalAuthorityOptions options)
{
    public Task<AuthenticationResult> LoginAsync(string email, string password, CancellationToken token)
    {
        if (!options.Configured) throw new IdentityFailure(503, "O acesso do portal aguarda configuração da autoridade própria.");
        return new AuthenticationService(store, hasher, options.Identity).LoginAsync(email, password, token);
    }
    public Task LogoutAsync(Guid id, CancellationToken token) => new AuthenticationService(store, hasher, options.Identity).LogoutAsync(id, token);
}
public static class PortalAuthority
{
    public const string Scheme = "OlivawPortal";
    public static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    public static void AddPortalAuthority(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("PortalIdentity").Get<global::Olivaw.Identity.Api.Security.IdentityOptions>() ?? new();
        var configured = !string.IsNullOrWhiteSpace(settings.Issuer) && !string.IsNullOrWhiteSpace(settings.Audience) && !string.IsNullOrWhiteSpace(settings.SigningKey) && Encoding.UTF8.GetByteCount(settings.SigningKey) >= 32 && settings.SigningKey != configuration["Identity:SigningKey"] && settings.Issuer != configuration["Identity:Issuer"] && settings.Audience != configuration["Identity:Audience"] && settings.TokenMinutes is >= 1 and <= 120;
        services.AddSingleton(new PortalAuthorityOptions { Configured = configured, Identity = settings });
        var validation = settings;
        if (!configured) validation = new() { Issuer = "urn:olivaw:portal:unconfigured", Audience = "urn:olivaw:portal:unconfigured", SigningKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)), TokenMinutes = 30 };
        services.AddDbContext<PortalIdentityDbContext>(options =>
        {
            var connection = configuration.GetConnectionString("PortalIdentity") ?? configuration.GetConnectionString("DbContext");
            var provider = configuration["DatabaseProvider"];
            var assembly = "Owh.Olivaw.Dados." + provider;
            if (provider == "PostgreSql") options.UseNpgsql(connection, providerOptions => { providerOptions.MigrationsAssembly(assembly); providerOptions.MigrationsHistoryTable("portalidentitymigrations", configuration["Database:Schema"]); });
            else if (provider == "SqlServer") options.UseSqlServer(connection, providerOptions => { providerOptions.MigrationsAssembly(assembly); providerOptions.MigrationsHistoryTable("portalidentitymigrations", configuration["Database:Schema"]); });
            else throw new InvalidOperationException("Provedor do portal não contemplado.");
            options.UseLowerCaseNamingConvention();
        });
        services.AddKeyedScoped<IIdentityStore>("portal", (provider, _) => new IdentityStore<PortalIdentityDbContext>(provider.GetRequiredService<PortalIdentityDbContext>()));
        services.AddScoped<PortalAuthentication>(); services.AddScoped<PortalAccess>();
        services.AddAuthentication().AddJwtBearer(Scheme, options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters { AuthenticationType = Scheme, ValidateIssuer = true, ValidIssuer = validation.Issuer, ValidateAudience = true, ValidAudience = validation.Audience, ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(validation.SigningKey)), ValidateLifetime = true, RequireExpirationTime = true, RequireSignedTokens = true, ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }, ClockSkew = TimeSpan.FromSeconds(30), NameClaimType = "name" };
            options.Events = new JwtBearerEvents { OnTokenValidated = async context =>
            {
                if (!configured || !Guid.TryParse(context.Principal?.FindFirstValue("sub"), out var id)) { context.Fail("Sessão inválida."); return; }
                var identity = context.HttpContext.RequestServices.GetRequiredKeyedService<IIdentityStore>("portal");
                var user = await identity.FindAsync<UserProfile>(item => item.ExternalId == id, context.HttpContext.RequestAborted);
                if (user == null || !user.IsActive || user.IsRoot || user.IsServiceAccount || context.Principal?.FindFirstValue("stamp") != user.SecurityStamp.ToString()) context.Fail("Sessão inválida.");
            }};
        });
        services.AddAuthorization(options => options.AddPolicy("portal.member", policy => policy.AddAuthenticationSchemes(Scheme).RequireAuthenticatedUser().RequireAssertion(context => context.User.Identities.Any(identity => identity.IsAuthenticated && identity.AuthenticationType == Scheme))));
    }
}
