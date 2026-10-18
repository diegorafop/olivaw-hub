using FluentValidation;
using Olivaw.Identity.Api;
using Owh.OlivawHub.Dados;
using MediatR;
using Owh.OlivawCore.Api.Extensions.Hostings;
using Owh.OlivawCore.Api.Validation;
using Owh.OlivawHub.Api.Extensions.Hostings;
using System.Diagnostics.CodeAnalysis;

namespace Owh.OlivawHub.Api.Extensions.Facades;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderFacadeExtensions
{
    public static void AddSetups(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization();
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
