using FluentValidation;
using MediatR;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class AlternarUsuarioCommand
    : EntityCommand,
      IRequest<AlternarUsuarioCommandResponse>
{
    public Guid Id { get; set; }
    public bool Ativo { get; set; }

    public AlternarUsuarioCommand() : base() { }
    public AlternarUsuarioCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }

}

public class AlternarUsuarioCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class AlternarUsuarioCommandHandler
(
    IRepository<Usuario> repository,
    AlternarUsuarioCommandValidator validator
) : IRequestHandler<AlternarUsuarioCommand,
    AlternarUsuarioCommandResponse>
{
    private readonly IRepository<Usuario> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public readonly AlternarUsuarioCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AlternarUsuarioCommandResponse> Handle
    (
        AlternarUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Usuario entity = await GetFirstAsync
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

        return entity.Map<AlternarUsuarioCommandResponse>();
    }

    private async Task<Usuario> GetFirstAsync
    (
        AlternarUsuarioCommand request,
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

    private async Task UpdateAsync
    (
        AlternarUsuarioCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        entity.Toggle(request.Ativo, request.UsuarioLogadoId);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class AlternarUsuarioCommandValidator
    : AbstractValidator<AlternarUsuarioCommand>
{
    public AlternarUsuarioCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AlternarUsuarioCommand, Guid>
                (item => item.Id));
    }
}