using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Helpers;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class UpsertDestinatarioExternalCommand
    : UpsertDestinatarioCommand,
      IRequest<UpsertDestinatarioExternalCommandResponse>
{ }

public class UpsertDestinatarioExternalCommandResponse
    : UpsertDestinatarioCommandResponse
{ }

public class UpsertDestinatarioExternalCommandHandler
(
    IServiceNotificacao notificacaoService
) : IRequestHandler<UpsertDestinatarioExternalCommand,
    UpsertDestinatarioExternalCommandResponse>
{
    private readonly IServiceNotificacao _service = notificacaoService
        ?? throw new ArgumentNullException(nameof(notificacaoService));

    public async Task<UpsertDestinatarioExternalCommandResponse> Handle
    (
        UpsertDestinatarioExternalCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var response = await UpsertDestinatarioAsync(request, cancellationToken);

        return response.ToUpsertDestinatarioExternalCommandResponse();
    }

    private async Task<UpsertDestinatarioCommandResponse> UpsertDestinatarioAsync
    (
        UpsertDestinatarioExternalCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _service.UpsertDestinatarioAsync
        (
            request,
            cancellationToken
        );
    }
}

public class UpsertDestinatarioCommandValidator
    : AbstractValidator<UpsertDestinatarioExternalCommand>
{
    public UpsertDestinatarioCommandValidator()
    {
        RuleFor(item => item.Nome)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<UpsertDestinatarioExternalCommand, string?>
                (item => item.Nome));

        RuleFor(item => item.Endereco)
            .NotEmpty()
            .WithMessage(MessageHelper
            .EmptyFor<UpsertDestinatarioExternalCommand, string>
            (item => item.Endereco));
    }
}