using Mps.Auth.Api.Extensions.Hostings;
using Mps.HolosCore.Api.Extensions.Hostings;
using System.Diagnostics.CodeAnalysis;

namespace Mps.Auth.Api.Extensions.Facades;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderFacadeExtensions
{
    public static void AddSetups
    (
        this WebApplicationBuilder builder
    )
    {
        builder.AddSecurity();
        builder.AddControllers();
        builder.AddSwagger();
        builder.AddFluentValidation();
        builder.AddMediatR();

        builder.AddSettingConfiguration();

        builder.AddMemoryCacheService();
        builder.AddServiceResponseValidator();

        builder.AddMassTransit(typeof(Program).Assembly);

        builder.AddServiceExternal();
        builder.AddServiceLogEngine();

        builder.AddDatabase();
    }
}