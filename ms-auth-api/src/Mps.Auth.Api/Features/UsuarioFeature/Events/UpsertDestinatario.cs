using MassTransit;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Contracts;

namespace Mps.Auth.Api.Features.UsuarioFeature.Events;

public class UpsertDestinatarioEventHandler
(
    IMediator mediator
) : IConsumer<EnfileirarUpsertDestinatarioContract>
{
    private readonly IMediator mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    public Task Consume
    (
        ConsumeContext<EnfileirarUpsertDestinatarioContract> context
    )
    {
        return mediator.Send(context.ToUpsertDestinatarioExternalCommand());
    }
}