using Microsoft.Extensions.Configuration;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Api.Settings;
using Owh.OlivawCore.Requests;
using Owh.OlivawCore.Services;
using Owh.OlivawCore.Settings;

namespace Owh.OlivawCore.Api.Extensions.Hostings;

public static class ConfigurationExtensions
{
    public static ISettingProviderAuthentication GetSettingProviderAuthentication
    (
        this ConfigurationManager configuration
    )
    {
        var setting = configuration
            .GetSection(SettingProviderAuthentication.ConfigurationSection)
            .Get<SettingProviderAuthentication>();

        return setting ?? throw new InvalidOperationException(MessageHelper
            .InvalidSettingFor<SettingProviderAuthentication>());
    }

    public static SettingMassTransit GetSettingMassTransit
    (
        this ConfigurationManager configuration
    )
    {
        var setting = configuration
            .GetSection(SettingMassTransit.ConfigurationSection)
            .Get<SettingMassTransit>();

        return setting ?? throw new InvalidOperationException(MessageHelper
            .InvalidSettingFor<SettingMassTransit>());
    }

    public static ISettingLoki GetSettingLoki
    (
        this ConfigurationManager configuration
    )
    {
        var setting = configuration
            .GetSection(SettingLoki.ConfigurationSection)
            .Get<SettingLoki>();

        return setting ?? throw new InvalidOperationException(MessageHelper
            .InvalidSettingFor<SettingLoki>());
    }

    public static ReadVersionQueryResponse GetReadVersionQueryResponse
    (
        this IConfiguration configuration
    )
    {
        return new ReadVersionQueryResponse()
        {
            Titulo = configuration.GetValueByKey<string>("swagger:title"),
            Versao = configuration.GetValueByKey<string>("swagger:version"),
            UltimaAtualizacao = configuration.GetValueByKey<string>("swagger:updatedAt")
        };
    }

    public static string GetServiceNameByTitle
    (
        this IConfiguration configuration
    )
    {
        var itens = configuration.GetManyServiceNameItem();

        if (itens.Length == 0)
            return "unknown";

        var environmentName = configuration.GetEnvironmentByTitle();

        return environmentName switch
        {
            "homologation" or "production" => GetServiceNameByItens(itens, 2),
            _ => GetServiceNameByItens(itens, 1),
        };
    }

    private static string GetServiceNameByItens
    (
        string[] itens,
        int offset
    )
    {
        if (itens.Length < offset)
            return "unknown";

        return string.Join("-", itens[..^offset]);
    }

    public static string GetEnvironmentByTitle
    (
        this IConfiguration configuration
    )
    {
        var itens = configuration.GetManyServiceNameItem();

        if (itens.Length == 0)
            return "unknown";

        string last = itens.LastOrDefault();

        if (string.IsNullOrEmpty(last))
            return "unknown";

        if (last.Contains("hml"))
            return "homologation";

        if (last.Contains("prd"))
            return "production";

        return last
            .ToLower()
            .Trim();
    }

    private static string[] GetManyServiceNameItem
    (
        this IConfiguration configuration
    )
    {
        var title = configuration.GetValueByKey<string>("swagger:title");

        if (string.IsNullOrEmpty(title))
            return [];

        return title.Split
        (
            ["-"],
            StringSplitOptions.None
        );
    }

    public static IEnumerable<string> GetManyScope
    (
        this IConfiguration configuration
    )
    {
        var section = configuration.GetSection
        (
            "keycloak-provider:scopes"
        );

        if (section is null)
            return [];

        return section.Get<string[]>();
    }

    public static T GetValueByKey<T>
    (
        this IConfiguration configuration,
        string key
    )
    {
        var value = configuration.GetValue<T>(key);

        if (value is null)
            return default;

        if (value is string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue))
                return (T)(object)string.Empty;

            return (T)(object)stringValue.Trim();
        }

        return value;
    }

    public static string GetValueByKey
    (
        this IConfiguration configuration,
        string key
    )
    {
        var value = configuration.GetValueByKey<string>(key);

        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value;
    }
}