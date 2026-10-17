using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mps.OlivawCore.Api.Extensions.Hostings;
using Mps.OlivawCore.Api.Settings;
using Mps.OlivawCore.Domain.Enum;
using System.Reflection;

namespace Mps.OlivawCore.Api.Extensions.Entities;

public static class MassTransitExtensions
{
    public static void AddMassTransit
    (
        this IServiceCollection services,
        ConfigurationManager configuration,
        Assembly assembly
    )
    {
        var setting = configuration.GetSettingMassTransit();

        if (setting.Configured)
            services.AddMassTransit
            (
                setting,
                assembly
            );
        else
            services.AddScoped<IPublishEndpoint>(provider => new NullPublishEndpoint());
    }

    public static void AddMassTransit
    (
        this IServiceCollection services,
        SettingMassTransit setting,
        Assembly assembly
    )
    {
        services.AddMassTransit(masstransit =>
        {
            if (setting.Consumer)
                masstransit.AddConsumers(assembly);

            masstransit.UsingByBrokerTypeEnum(setting);
        });
    }

    public static void UsingByBrokerTypeEnum
    (
        this IBusRegistrationConfigurator masstransit,
        SettingMassTransit setting
    )
    {
        if (setting.RabbitMq())
            masstransit.UsingRabbitMq(setting);
        else if (setting.ActiveMq())
            masstransit.UsingActiveMq(setting);
        else if (setting.AzureServiceBus())
            masstransit.UsingAzureServiceBus(setting);
        else
            throw new InvalidOperationException(nameof(masstransit));
    }

    public static void UsingRabbitMq
    (
        this IBusRegistrationConfigurator masstransit,
        SettingMassTransit setting
    )
    {
        masstransit.UsingRabbitMq((context, rabbit) =>
        {
            rabbit.SetOlivawCoreEntityNameFormatter(setting);
            rabbit.Host
            (
                setting.Server,
                (ushort)setting.Port,
                setting.VirtualHost,
                host =>
                {
                    host.Username(setting.User);
                    host.Password(setting.Password);
                }
            );

            rabbit.ConfigureEndpoints
            (
                context,
                setting.GetKebabCaseEndpointNameFormatter()
            );
        });
    }

    public static void SetOlivawCoreEntityNameFormatter
    (
        this IRabbitMqBusFactoryConfigurator rabbitMq,
        SettingMassTransit setting
    )
    {
        rabbitMq
            .MessageTopology
            .SetEntityNameFormatter
            (
                new OlivawCoreEntityNameFormatter
                (
                    rabbitMq
                        .MessageTopology
                        .EntityNameFormatter,
                    setting.Prefix
                )
            );
    }

    public static void SetOlivawCoreEntityNameFormatter
    (
        this IActiveMqBusFactoryConfigurator activeMq,
        SettingMassTransit setting
    )
    {
        activeMq
            .MessageTopology
            .SetEntityNameFormatter
            (
                new OlivawCoreEntityNameFormatter
                (
                    activeMq
                        .MessageTopology
                        .EntityNameFormatter,
                    setting.Prefix
                )
            );
    }

    public static void SetOlivawCoreEntityNameFormatter
    (
        this IServiceBusBusFactoryConfigurator serviceBus,
        SettingMassTransit setting
    )
    {
        serviceBus
            .MessageTopology
            .SetEntityNameFormatter
            (
                new OlivawCoreEntityNameFormatter
                (
                    serviceBus
                        .MessageTopology
                        .EntityNameFormatter,
                    setting.Prefix
                )
            );
    }

    internal static void UsingActiveMq
    (
        this IBusRegistrationConfigurator masstransit,
        SettingMassTransit setting
    )
    {
        masstransit.UsingActiveMq((context, activeMq) =>
        {
            activeMq.SetOlivawCoreEntityNameFormatter(setting);

            activeMq.Host
            (
                setting.Server,
                setting.Port,
                host =>
                {
                    host.Username(setting.User);
                    host.Password(setting.Password);
                }
            );

            activeMq.EnableArtemisCompatibility();

            activeMq.ConfigureEndpoints
            (
                context,
                setting.GetKebabCaseEndpointNameFormatter()
            );
        });
    }

    internal static void UsingAzureServiceBus
    (
        this IBusRegistrationConfigurator masstransit,
        SettingMassTransit setting
    )
    {
        masstransit.UsingAzureServiceBus((context, serviceBus) =>
        {
            serviceBus.SetOlivawCoreEntityNameFormatter(setting);

            serviceBus.Host(setting.Server);

            serviceBus.ConfigureEndpoints
            (
                context,
                setting.GetKebabCaseEndpointNameFormatter()
            );
        });
    }

    internal static bool AzureServiceBus
    (
        this SettingMassTransit setting
    )
    {
        return setting
            .BrokerTypeEnum
            .Equals(BrokerTypeEnum.AzureServiceBus);
    }

    internal static bool ActiveMq
    (
        this SettingMassTransit setting
    )
    {
        return setting
            .BrokerTypeEnum
            .Equals(BrokerTypeEnum.ActiveMq);
    }

    internal static bool RabbitMq
    (
        this SettingMassTransit setting
    )
    {
        return setting
            .BrokerTypeEnum
            .Equals(BrokerTypeEnum.RabbitMq);
    }
}