using FluentValidation;
using Olivaw.Identity.Api;
using Mps.Ltmj.Dados;
using MediatR;
using Mps.OlivawCore.Api.Extensions.Hostings;
using Mps.OlivawCore.Api.Validation;
using Mps.Ltmj.Api.Extensions.Hostings;
using System.Diagnostics.CodeAnalysis;

namespace Mps.Ltmj.Api.Extensions.Facades;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderFacadeExtensions
{
    public static void AddSetups(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options => options.AddPolicy("portfolio.manage", policy =>
            policy.AddAuthenticationSchemes("OlivawIdentity").RequireAuthenticatedUser()
                .AddRequirements(new Olivaw.Identity.Api.Security.PermissionRequirement("portfolio.manage"))));
        builder.Services.Configure<Mps.Ltmj.Api.Settings.PortfolioSettings>(builder.Configuration.GetSection("Portfolio"));
        builder.Services.AddScoped<Mps.Ltmj.Portfolio.IPhotoStorage, Mps.Ltmj.Api.Services.LocalPhotoStorage>();
        builder.Services.AddScoped<Mps.Ltmj.Portfolio.IPortfolioReader, Mps.Ltmj.Dados.Repositories.PortfolioReader>();
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
