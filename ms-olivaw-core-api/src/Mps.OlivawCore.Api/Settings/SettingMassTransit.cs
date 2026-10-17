using MassTransit;
using MassTransit.Configuration;
using Mps.OlivawCore.Domain.Enum;
using System.Diagnostics.CodeAnalysis;

namespace Mps.OlivawCore.Api.Settings;

public class SettingMassTransit
{
    public const string ConfigurationSection = "masstransit";

    public bool Configured { get; set; }
    public bool Consumer { get; set; }

    public BrokerTypeEnum BrokerTypeEnum { get; set; }

    public string Server { get; set; }
    public int Port { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string Prefix { get; set; }
    public string Environment { get; set; }
    public string VirtualHost { get { return GetVirtualHost(); } set => _virtualHost = value; }

    private string _virtualHost;
    private string GetVirtualHost()
    {
        if (string.IsNullOrWhiteSpace(_virtualHost))
            return "/";
        return _virtualHost;
    }

    public KebabCaseEndpointNameFormatter GetKebabCaseEndpointNameFormatter()
    {
        if (string.IsNullOrWhiteSpace(Prefix))
            return null;

        return new KebabCaseEndpointNameFormatter($"{Prefix}.", false);
    }
}

[ExcludeFromCodeCoverage]
public class NullPublishEndpoint : IPublishEndpoint
{
    public ConnectHandle ConnectPublishObserver
    (
        IPublishObserver observer
    )
    {
        throw new NotImplementedException("NullPublishEndpoint does not support observers.");
    }

    public Task Publish<T>
    (
        T message,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        return Task.CompletedTask;
    }

    public Task Publish
    (
        object message,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }

    public Task Publish
    (
        object message,
        IPublishTopologyConvention convention,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }

    public Task Publish<T>
    (
        object message,
        IPublishTopologyConvention convention,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        return Task.CompletedTask;
    }

    public Task Publish<T>
    (
        T message,
        IPipe<PublishContext<T>> publishPipe,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        return Task.CompletedTask;
    }

    public Task Publish<T>
    (
        T message,
        IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        return Task.CompletedTask;
    }

    public Task Publish
    (
        object message,
        IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }

    public Task Publish
    (
        object message,
        Type messageType,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }

    public Task Publish
    (
        object message,
        Type messageType,
        IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }

    public Task Publish<T>
    (
        object values,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        return Task.CompletedTask;
    }

    public Task Publish<T>
    (
        object values,
        IPipe<PublishContext<T>> publishPipe,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        return Task.CompletedTask;
    }

    public Task Publish<T>
    (
        object values,
        IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        return Task.CompletedTask;
    }
}
