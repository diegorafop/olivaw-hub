using FluentValidation;
using Olivaw.Identity.Api;
using Mps.Devops.Dados;
using MediatR;
using Mps.OlivawCore.Api.Extensions.Hostings;
using Mps.OlivawCore.Api.Validation;
using Mps.Devops.Api.Extensions.Hostings;
using System.Diagnostics.CodeAnalysis;

namespace Mps.Devops.Api.Extensions.Facades;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderFacadeExtensions
{
    public static void AddSetups(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<Mps.Devops.Api.Features.ImplantacaoFeature.Internals.DeploymentGate>();
        builder.Services.Configure<Mps.Devops.Api.Settings.DeploymentSettings>(builder.Configuration.GetSection("Deployment"));
        builder.Services.AddOptions<Mps.Devops.Api.Settings.DeploymentSettings>()
            .Validate(item => item.TimeoutSeconds > 0, "Deployment:TimeoutSeconds deve ser positivo.")
            .Validate(item => item.TimeoutSeconds <= 3600, "Deployment:TimeoutSeconds deve ser no máximo 3600.")
            .ValidateOnStart();
        builder.Services.AddScoped<Mps.Devops.Services.IProcessExecutionService, Mps.Devops.Api.Services.ProcessExecutionService>();
        builder.Services.AddAuthorization(options => options.AddPolicy("devops.implantar", policy =>
            policy.AddAuthenticationSchemes(Olivaw.Identity.Api.IdentityRegistration.Scheme).RequireAuthenticatedUser()
                .AddRequirements(new Olivaw.Identity.Api.Security.PermissionRequirement("devops.implantar"))));
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
    }
}
