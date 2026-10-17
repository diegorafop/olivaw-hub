using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.PerfilAcessoFeature.Commands;

public class AtualizarPerfilAcessoCommand
    : EntityCommand,
      IRequest<AtualizarPerfilAcessoCommandResponse>
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string? Descricao { get; set; }

    public AtualizarPerfilAcessoCommand() : base() { }
    public AtualizarPerfilAcessoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AtualizarPerfilAcessoCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
}

public class AtualizarPerfilAcessoCommandHandler
(
    IRepository<PerfilAcesso> repository,
    AtualizarPerfilAcessoCommandValidator validator
) : IRequestHandler<AtualizarPerfilAcessoCommand,
    AtualizarPerfilAcessoCommandResponse>
{
    private readonly IRepository<PerfilAcesso> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly AtualizarPerfilAcessoCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AtualizarPerfilAcessoCommandResponse> Handle
    (
        AtualizarPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        await ExistsAsync(request, cancellationToken);
        PerfilAcesso entity = await GetFirstAsync(request, cancellationToken);
        request.Update(entity);
        await UpdateAsync(entity, request, cancellationToken);

        return entity.Map<AtualizarPerfilAcessoCommandResponse>();
    }

    private async Task ExistsAsync
    (
        AtualizarPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        bool exists = await _repository.ExistsAsync
        (
            item => item
                .Nome
                .ToUpper()
                .Equals(request
                    .Nome
                    .ToUpper())
                && !item
                    .IdExterno
                    .Equals(request.Id),
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            exists,
            cancellationToken
        );
    }

    private async Task<PerfilAcesso> GetFirstAsync
    (
        AtualizarPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            item => item.IdExterno.Equals(request.Id),
            cancellationToken
        );
    }

    private async Task UpdateAsync
    (
        PerfilAcesso entity,
        AtualizarPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        entity.UsuarioLogadoId = request.UsuarioLogadoId;
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class AtualizarPerfilAcessoCommandValidator
    : AbstractValidator<AtualizarPerfilAcessoCommand>
{
    public AtualizarPerfilAcessoCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarPerfilAcessoCommand, Guid>
                (item => item.Id));

        RuleFor(item => item.Nome)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarPerfilAcessoCommand, string>
                (item => item.Nome));
    }

    public async Task ValidateDataAsync
    (
        AtualizarPerfilAcessoCommand request,
        bool exists,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.Nome)
            .Must(item => { return !exists; })
            .WithMessage(MessageHelper
                .DuplicateKeyFor(request, item => item.Nome));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}