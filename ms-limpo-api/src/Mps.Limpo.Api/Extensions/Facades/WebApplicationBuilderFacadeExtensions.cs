using FluentValidation;
using MediatR;
using Mps.HolosCore.Api.Extensions.Hostings;
using Mps.HolosCore.Api.Validation;
using Mps.Limpo.Api.Extensions.Hostings;
using System.Diagnostics.CodeAnalysis;

namespace Mps.Limpo.Api.Extensions.Facades;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderFacadeExtensions
{
    public static void AddSetups(this WebApplicationBuilder builder)
    {
        builder.AddSecurityCore();
        builder.AddControllerCore();
        builder.AddSwagger();
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FluentValidatorBehavior<,>));
        builder.Services.AddMediatRCore(typeof(Program).Assembly);
        builder.AddMassTransit(typeof(Program).Assembly);
        builder.AddMemoryCacheService();
        builder.AddSettingConfigurationCore();
        builder.AddServiceResponseValidator();
        builder.AddServiceExternalCore();
        builder.AddServiceLogEngine();
        builder.AddDatabase();
    }
}
