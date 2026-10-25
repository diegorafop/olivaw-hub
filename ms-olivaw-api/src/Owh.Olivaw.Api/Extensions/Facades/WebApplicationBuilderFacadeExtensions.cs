using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using FluentValidation;
using System.Threading.RateLimiting;
using Olivaw.Identity.Api;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Dados;
using MediatR;
using Owh.OlivawCore.Api.Extensions.Hostings;
using Owh.OlivawCore.Api.Validation;
using Owh.Olivaw.Api.Extensions.Hostings;
using System.Diagnostics.CodeAnalysis;

namespace Owh.Olivaw.Api.Extensions.Facades;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderFacadeExtensions
{
    public static void AddSetups(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options => options.AddPolicy("olivaw.root", policy =>
            policy.RequireAuthenticatedUser().RequireClaim("root", "true")));
        builder.Services.AddAuthorization(options => options.AddPolicy("leads.manage", policy =>
            policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement("leads.manage"))));
        builder.Services.AddAuthorization(options => options.AddPolicy("security.reports.write", policy =>
            policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement("security.reports.write"))));
        builder.Services.AddAuthorization(options => options.AddPolicy("security.remediations.execute", policy =>
            policy.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement("security.remediations.execute"))));
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("security-report-write", context => RateLimitPartition.GetFixedWindowLimiter(
                context.User.FindFirst("sub")?.Value ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 12, Window = TimeSpan.FromHours(1), QueueLimit = 0 }));
            options.AddPolicy("lead-write", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
        });
        builder.Services.AddHttpClient<Owh.Olivaw.Api.Services.PublicHealthProbe>(client => client.Timeout = TimeSpan.FromSeconds(5))
            .ConfigurePrimaryHttpMessageHandler(Owh.Olivaw.Api.Services.PublicHealthProbe.CreateHandler);
        builder.Services.AddCorsDefaultPolicyCore();
        builder.AddControllerCore();
        builder.AddSwagger();
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TemplateValidationBehavior<,>));
        builder.Services.AddMediatRCore(typeof(Program).Assembly);
        builder.AddMassTransit(typeof(Program).Assembly);
        builder.AddMemoryCacheService();
        builder.AddSettingConfigurationCore();
        builder.AddServiceResponseValidator();

        builder.AddServiceLogEngine();
        builder.AddDatabase();
        builder.Services.AddOlivawIdentity<WriteDbContext>(builder.Configuration);
        builder.Services.AddPortalAuthority(builder.Configuration);
    }
}
