using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.PerfilAcessoFeature.Commands;

public class AlternarPerfilAcessoCommand
    : IRequest<AlternarPerfilAcessoCommandResponse>
{
    public Guid Id { get; set; }
    public bool Ativo { get; set; }

    internal Guid UsuarioLogadoId { get; set; }
    public AlternarPerfilAcessoCommand() { }
    public AlternarPerfilAcessoCommand
    (
        Guid usuarioLogadoId
    )
    {
        UsuarioLogadoId = usuarioLogadoId;
    }
}

public class AlternarPerfilAcessoCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class AlternarPerfilAcessoCommandHandler
(
    IRepository<PerfilAcesso> repository
) : IRequestHandler<AlternarPerfilAcessoCommand,
    AlternarPerfilAcessoCommandResponse>
{
    private readonly IRepository<PerfilAcesso> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<AlternarPerfilAcessoCommandResponse> Handle
    (
        AlternarPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        PerfilAcesso entity = await GetFirstAsync
        (
            request,
            cancellationToken
        );

        await UpdateAsync
        (
            request,
            entity,
            cancellationToken
        );

        return entity.Map<AlternarPerfilAcessoCommandResponse>();
    }

    private async Task UpdateAsync
    (
        AlternarPerfilAcessoCommand request,
        PerfilAcesso entity,
        CancellationToken cancellationToken
    )
    {
        entity.Toggle(request.Ativo, request.UsuarioLogadoId);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<PerfilAcesso> GetFirstAsync
    (
        AlternarPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.Id),
            cancellationToken
        );
    }
}

public class AlternarPerfilAcessoCommandValidator
    : AbstractValidator<AlternarPerfilAcessoCommand>
{
    public AlternarPerfilAcessoCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AlternarPerfilAcessoCommand, Guid>
                (item => item.Id));
    }
}