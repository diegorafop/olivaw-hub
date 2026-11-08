using MassTransit;
using Owh.OlivawCore.Contracts;

namespace Owh.OlivawCore.Api.Extensions.Entities;

public static class BaseContractExtensions
{
    public static Guid GetId
    (
        this ConsumeContext<BaseContract> context
    )
    {
        if (context.Message is null)
            return default;

        return context
            .Message
            .Id;
    }

    public static Guid GetTraceId
    (
        this ConsumeContext<BaseContract> context
    )
    {
        if (context.Message is null)
            return default;

        return context
            .Message
            .TraceId;
    }
}