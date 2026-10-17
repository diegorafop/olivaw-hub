using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class AlterarSenhaByExternoCommand
     : EntityCommand,
       IRequest<AlterarSenhaByExternoCommandResponse>
{
    public Guid UsuarioId { get; set; }
    public string SenhaNova { get; set; }
    public string SenhaConfirmacao { get; set; }

    public AlterarSenhaByExternoCommand() : base() { }
    public AlterarSenhaByExternoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AlterarSenhaByExternoCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
}

public class AlterarSenhaByExternoCommandHandler
(
    IRepositoryUsuario repository,
    AlterarSenhaByExternoCommandValidator validator
) : IRequestHandler<AlterarSenhaByExternoCommand,
    AlterarSenhaByExternoCommandResponse>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly AlterarSenhaByExternoCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AlterarSenhaByExternoCommandResponse> Handle
    (
        AlterarSenhaByExternoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Usuario entity = await GetFirstAsync(request, cancellationToken);
        await UpdateAsync(request, entity, cancellationToken);

        return entity.Map<AlterarSenhaByExternoCommandResponse>();
    }

    private async Task<Usuario> GetFirstAsync
    (
        AlterarSenhaByExternoCommand request,
        CancellationToken cancellationToken
    )
    {
        var entity = await _repository.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.UsuarioId),
            cancellationToken
        );
        return entity;
    }

    private async Task UpdateAsync
    (
        AlterarSenhaByExternoCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        request.UpdateSenha(entity);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class AlterarSenhaByExternoCommandValidator
    : AbstractValidator<AlterarSenhaByExternoCommand>
{
    public AlterarSenhaByExternoCommandValidator()
    {
        RuleFor(item => item.UsuarioId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AlterarSenhaByExternoCommand, Guid>
                (item => item.UsuarioId));

        RuleFor(item => item.SenhaNova)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AlterarSenhaByExternoCommand, string>
                (item => item.SenhaNova));

        RuleFor(item => item.SenhaConfirmacao)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AlterarSenhaByExternoCommand, string>
                (item => item.SenhaConfirmacao));

        RuleFor(item => item.SenhaConfirmacao)
            .Equal(item => item.SenhaNova)
            .WithMessage(MessageHelper
                .EqualFor<AlterarSenhaByExternoCommand, string, string>
                (
                    item => item.SenhaNova,
                    item => item.SenhaConfirmacao
                ));
    }
}