using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Owh.OlivawCore.Api.Extensions.Entities;
using Owh.OlivawCore.Api.Settings;
using Owh.OlivawCore.Contracts;
using Owh.OlivawCore.Domain.Enum;
using Xunit;

namespace Owh.OlivawCore.Tests.SettingsTest;

public class MassTransitSettingsTest
{
    [Theory]
    [InlineData(null, "/")]
    [InlineData("", "/")]
    [InlineData(" ", "/")]
    [InlineData("olivaw", "olivaw")]
    public void VirtualHost_should_default_to_root_only_when_missing(string? value, string expected)
    {
        Assert.Equal(expected, new SettingMassTransit { VirtualHost = value! }.VirtualHost);
    }

    [Fact]
    public void Endpoint_formatter_should_apply_prefix_and_kebab_case()
    {
        Assert.Null(new SettingMassTransit().GetKebabCaseEndpointNameFormatter());
        var formatter = new SettingMassTransit { Prefix = "olivaw" }.GetKebabCaseEndpointNameFormatter();
        Assert.Equal("olivaw.order-submitted", formatter.Message<OrderSubmitted>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void OlivawCoreEntityNameFormatter_should_reject_missing_prefix(string? prefix)
    {
        Assert.Throws<ArgumentNullException>(() => new OlivawCoreEntityNameFormatter(Mock.Of<IEntityNameFormatter>(), prefix!));
    }

    [Fact]
    public void FormatEntityName_should_keep_only_message_name_and_prepend_prefix()
    {
        var original = new Mock<IEntityNameFormatter>();
        original.Setup(item => item.FormatEntityName<OrderSubmitted>()).Returns("Namespace:OrderSubmitted");
        Assert.Equal("olivaw-OrderSubmitted", new OlivawCoreEntityNameFormatter(original.Object, "olivaw").FormatEntityName<OrderSubmitted>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Contract_extensions_should_read_identifiers_or_return_empty_for_missing_message(bool hasMessage)
    {
        var message = new OrderSubmitted { Id = Guid.NewGuid(), TraceId = Guid.NewGuid() };
        var context = new Mock<ConsumeContext<BaseContract>>();
        context.SetupGet(item => item.Message).Returns(hasMessage ? message : null!);
        Assert.Equal(hasMessage ? message.Id : Guid.Empty, context.Object.GetId());
        Assert.Equal(hasMessage ? message.TraceId : Guid.Empty, context.Object.GetTraceId());
    }

    [Fact]
    public async Task AddMassTransit_should_register_noop_publisher_when_disabled()
    {
        using var configuration = new ConfigurationManager();
        configuration["masstransit:Configured"] = "false";
        var services = new ServiceCollection();
        services.AddMassTransit(configuration, typeof(Program).Assembly);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var publisher = Assert.IsType<NullPublishEndpoint>(scope.ServiceProvider.GetRequiredService<IPublishEndpoint>());
        await publisher.Publish(new OrderSubmitted(), TestContext.Current.CancellationToken);
        Assert.Throws<NotImplementedException>(() => publisher.ConnectPublishObserver(Mock.Of<IPublishObserver>()));
        Assert.Same(publisher, scope.ServiceProvider.GetRequiredService<IPublishEndpoint>());
    }

    [Fact]
    public void UsingByBrokerTypeEnum_should_reject_unknown_broker()
    {
        Assert.Throws<InvalidOperationException>(() => Mock.Of<IBusRegistrationConfigurator>()
            .UsingByBrokerTypeEnum(new SettingMassTransit { BrokerTypeEnum = BrokerTypeEnum.None }));
    }

    [Theory]
    [InlineData(BrokerTypeEnum.RabbitMq)]
    [InlineData(BrokerTypeEnum.ActiveMq)]
    [InlineData(BrokerTypeEnum.AzureServiceBus)]
    public void AddMassTransit_should_register_supported_brokers_without_starting_them(BrokerTypeEnum broker)
    {
        var services = new ServiceCollection();

        var setting = new SettingMassTransit
        {
            Configured = true,
            BrokerTypeEnum = broker,
            Prefix = "olivaw",
            Server = "localhost",
            Port = 5672
        };

        services.AddMassTransit(setting, typeof(Program).Assembly);
        Assert.Contains(services, item => item.ServiceType == typeof(IBus));
        Assert.Contains(services, item => item.ServiceType == typeof(IPublishEndpoint));
    }

    [Fact]
    public void SetOlivawCoreEntityNameFormatter_should_wrap_each_broker_topology()
    {
        var setting = new SettingMassTransit { Prefix = "olivaw" };
        var rabbit = new Mock<IRabbitMqBusFactoryConfigurator> { DefaultValue = DefaultValue.Mock };
        rabbit.Object.SetOlivawCoreEntityNameFormatter(setting);
        rabbit.Verify(item => item.MessageTopology.SetEntityNameFormatter(It.IsAny<OlivawCoreEntityNameFormatter>()), Times.Once);
        var active = new Mock<IActiveMqBusFactoryConfigurator> { DefaultValue = DefaultValue.Mock };
        active.Object.SetOlivawCoreEntityNameFormatter(setting);
        active.Verify(item => item.MessageTopology.SetEntityNameFormatter(It.IsAny<OlivawCoreEntityNameFormatter>()), Times.Once);
        var azure = new Mock<IServiceBusBusFactoryConfigurator> { DefaultValue = DefaultValue.Mock };
        azure.Object.SetOlivawCoreEntityNameFormatter(setting);
        azure.Verify(item => item.MessageTopology.SetEntityNameFormatter(It.IsAny<OlivawCoreEntityNameFormatter>()), Times.Once);
    }

    public class OrderSubmitted : BaseContract { }
}