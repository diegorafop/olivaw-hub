using MassTransit;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Contracts;

namespace Mps.Auth.Api.Features.UsuarioFeature.Events;

public class UpsertDocumentoUsuarioPreferenciaEventHandler
(
    IMediator mediator
) : IConsumer<EnfileirarUpsertDocumentoUsuarioPreferenciaContract>
{
    private readonly IMediator mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    public Task Consume
    (
        ConsumeContext<EnfileirarUpsertDocumentoUsuarioPreferenciaContract> context
    )
    {
        return mediator.Send
        (
            context.ToUpsertDocumentoUsuarioPreferenciaExternalCommand()
        );
    }
}