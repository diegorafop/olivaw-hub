using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Owh.OlivawCore.Api.Extensions.Entities;
using Owh.OlivawCore.Domain.Enum;
using Xunit;

namespace Owh.OlivawCore.Tests.SettingsTest;

public class BrokerConfigurationTest
{
    [Theory]
    [InlineData(BrokerTypeEnum.RabbitMq, "localhost", 5672, "rabbitmq")]
    [InlineData(BrokerTypeEnum.ActiveMq, "localhost", 61616, "activemq")]
    [InlineData(BrokerTypeEnum.AzureServiceBus, "sb://olivaw-test.servicebus.windows.net/", 0, "sb")]
    public async Task AddMassTransit_should_build_transport_and_topology_without_starting_bus(
        BrokerTypeEnum broker, string server, int port, string scheme)
    {
        using var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["masstransit:Configured"] = "true",
            ["masstransit:BrokerTypeEnum"] = broker.ToString(),
            ["masstransit:Server"] = server,
            ["masstransit:Port"] = port.ToString(),
            ["masstransit:Prefix"] = "olivaw",
            ["masstransit:User"] = "test-user",
            ["masstransit:Password"] = "dummy-not-a-secret"
        });
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMassTransit(configuration, typeof(Program).Assembly);
        await using var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<IBus>();
        Assert.Equal(scheme, bus.Address.Scheme);
        Assert.NotNull(provider.GetRequiredService<IBusControl>());
    }
}