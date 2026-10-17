using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.VersaoFeature.Queries;
using Mps.Auth.Api.Settings;
using Mps.Auth.Settings;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Extensions.Hostings;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Requests;

namespace Mps.Auth.Api.Extensions.Hostings;

public static class ConfigurationExtensions
{
    public static ISettingUsuario GetSettingUsuario
    (
        this ConfigurationManager configuration
    )
    {
        var setting = configuration
            .GetSection(SettingUsuario.ConfigurationSection)
            .Get<SettingUsuario>();

        return setting ?? throw new InvalidOperationException(MessageHelper
            .InvalidSettingFor<SettingUsuario>());
    }

    public static ISettingDocumento GetSettingDocumento
    (
        this ConfigurationManager configuration
    )
    {
        var setting = configuration
            .GetSection(SettingDocumento.ConfigurationSection)
            .Get<SettingDocumento>();

        return setting ?? throw new InvalidOperationException(MessageHelper
            .InvalidSettingFor<SettingDocumento>());
    }

    public static ISettingNotificacao GetSettingNotificacao
    (
        this ConfigurationManager configuration
    )
    {
        var setting = configuration
            .GetSection(SettingNotificacao.ConfigurationSection)
            .Get<SettingNotificacao>();

        return setting ?? throw new InvalidOperationException(MessageHelper
            .InvalidSettingFor<SettingNotificacao>());
    }

    public static bool GetKeyCloakProviderConfigured
    (
        this ConfigurationManager configuration
    )
    {
        return configuration.GetValue<bool>("keycloak-provider:configured");
    }

    public static SelecionarVersaoExternalQueryResponse GetSelecionarExternalVersaoQueryResponse
    (
        this IConfiguration configuration
    )
    {
        var response = configuration.GetSelecionarVersaoQueryResponse();

        return EntityExtensions
            .Convert<SelecionarVersaoQueryResponse, 
                SelecionarVersaoExternalQueryResponse>(response);
    }

    public static string GetTipoByIssuer
    (
        this IConfiguration configuration
    )
    {
        var value = configuration.GetValue<string>
        (
            "authentication:oidc:custom-claims:emissor"
        );

        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value;
    }
}