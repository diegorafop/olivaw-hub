using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Helpers;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class UpsertDocumentoUsuarioPreferenciaExternalCommand
    : UpsertDocumentoUsuarioPreferenciaCommand,
      IRequest<UpsertDocumentoUsuarioPreferenciaExternalCommandResponse>
{ }

public class UpsertDocumentoUsuarioPreferenciaExternalCommandResponse
    : UpsertDocumentoUsuarioPreferenciaCommandResponse
{ }

public class UpsertDocumentoUsuarioPreferenciaExternalCommandHandler
(
    IServiceUsuario service
) : IRequestHandler<UpsertDocumentoUsuarioPreferenciaExternalCommand,
    UpsertDocumentoUsuarioPreferenciaExternalCommandResponse>
{
    private readonly IServiceUsuario _service = service
        ?? throw new ArgumentNullException(nameof(service));

    public async Task<UpsertDocumentoUsuarioPreferenciaExternalCommandResponse> Handle
    (
        UpsertDocumentoUsuarioPreferenciaExternalCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var response = await UpsertDocumentoUsuarioPreferenciaAsync
        (
            request,
            cancellationToken
        );

        return response.ToUpsertDocumentoUsuarioPreferenciaExternalCommandResponse();
    }

    private async Task<UpsertDocumentoUsuarioPreferenciaCommandResponse>
        UpsertDocumentoUsuarioPreferenciaAsync
    (
        UpsertDocumentoUsuarioPreferenciaExternalCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _service.UpsertDocumentoUsuarioPreferenciaAsync
        (
            request,
            cancellationToken
        );
    }
}

public class UpsertDocumentoUsuarioPreferenciaCommandValidator
    : AbstractValidator<UpsertDocumentoUsuarioPreferenciaExternalCommand>
{
    public UpsertDocumentoUsuarioPreferenciaCommandValidator()
    {
        RuleFor(item => item.UsuarioId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<UpsertDocumentoUsuarioPreferenciaExternalCommand, Guid>
                (item => item.UsuarioId));
    }
}