using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Helpers;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class RecuperarSenhaCommand
    : IRequest<RecuperarSenhaCommandResponse>
{
    public string Email { get; set; }

}

public class RecuperarSenhaCommandResponse
{
    public string MensagemValidacao { get; set; }
}

public class RecuperarSenhaCommandHandler
(
    IRepositoryUsuario repository,
    IServiceNotificacao serviceNotificacao,
    IConfiguration configuration
) : IRequestHandler<RecuperarSenhaCommand,
    RecuperarSenhaCommandResponse>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IServiceNotificacao _service = serviceNotificacao
        ?? throw new ArgumentNullException(nameof(serviceNotificacao));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    public async Task<RecuperarSenhaCommandResponse> Handle
    (
        RecuperarSenhaCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Usuario entity = await GetFirstAsync(request, cancellationToken);

        await EnfileirarEnviarEmailByEnderecoAsync(entity, cancellationToken);

        return request.ToRecuperarSenhaCommandResponse();
    }

    private async Task EnfileirarEnviarEmailByEnderecoAsync
    (
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        if (!_service.Configured)
            return;

        var request = entity.ToEnfileirarEnviarEmailByEnderecoCommand(_configuration);

        await _service.EnfileirarEnviarEmailByEnderecoAsync
        (
            request,
            cancellationToken
        );
    }

    private async Task<Usuario> GetFirstAsync
    (
        RecuperarSenhaCommand request,
        CancellationToken cancellationToken
    )
    {
        var entity = await _repository.GetFirstAsync
        (
            item => item.Email.Equals(request.Email),
            cancellationToken
        );

        return entity;
    }
}

public class RecuperarSenhaCommandValidator
    : AbstractValidator<RecuperarSenhaCommand>
{
    public RecuperarSenhaCommandValidator()
    {
        RuleFor(item => item.Email)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<RecuperarSenhaCommand, string>
                (item => item.Email));

        RuleFor(item => item.Email)
            .EmailAddress()
            .WithMessage(MessageHelper
                .EmailFor<RecuperarSenhaCommand, string>
                (item => item.Email));
    }
}