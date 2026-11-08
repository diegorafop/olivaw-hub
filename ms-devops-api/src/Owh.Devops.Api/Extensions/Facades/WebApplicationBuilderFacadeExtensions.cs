using FluentValidation;
using Olivaw.Identity.Api;
using Owh.Devops.Dados;
using MediatR;
using Owh.OlivawCore.Api.Extensions.Hostings;
using Owh.OlivawCore.Api.Validation;
using Owh.Devops.Api.Extensions.Hostings;
using System.Diagnostics.CodeAnalysis;

namespace Owh.Devops.Api.Extensions.Facades;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderFacadeExtensions
{
    public static void AddSetups(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<Owh.Devops.Api.Settings.DeploymentSettings>(builder.Configuration.GetSection("Deployment"));
        builder.Services.AddOptions<Owh.Devops.Api.Settings.DeploymentSettings>()
            .Validate(item => item.TimeoutSeconds > 0, "Deployment:TimeoutSeconds deve ser positivo.")
            .Validate(item => item.TimeoutSeconds <= 120, "Deployment:TimeoutSeconds deve ser no máximo 120.")
            .ValidateOnStart();
        builder.Services.AddScoped<Owh.Devops.Services.IDiskCleanupService, Owh.Devops.Api.Services.SshDeploymentService>();
        builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Owh.Devops.Api.Security.RootAccessHandler>();
        builder.Services.AddAuthorization(options => options.AddPolicy("devops.root", policy =>
            policy.AddAuthenticationSchemes(Olivaw.Identity.Api.IdentityRegistration.Scheme).RequireAuthenticatedUser()
                .AddRequirements(new Owh.Devops.Api.Security.RootAccessRequirement())));
        builder.Services.AddScoped<Owh.Devops.Services.IDeploymentAccessService, Owh.Devops.Api.Services.DeploymentAccessService>();
        builder.Services.AddScoped<Owh.Devops.Services.IDeploymentService, Owh.Devops.Api.Services.SshDeploymentService>();
        builder.Services.AddScoped<Owh.Devops.Services.IProcessExecutionService, Owh.Devops.Api.Services.ProcessExecutionService>();
        builder.Services.AddAuthorization(options => options.AddPolicy("devops.deploy", policy =>
            policy.AddAuthenticationSchemes(Olivaw.Identity.Api.IdentityRegistration.Scheme).RequireAuthenticatedUser()
                .AddRequirements(new Olivaw.Identity.Api.Security.PermissionRequirement("devops.deploy"))));
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
        builder.Services.AddOlivawIdentityConsumer(builder.Configuration);
    }
}
