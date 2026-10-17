using FluentValidation;
using MediatR;
using Mps.Limpo.Api.Extensions.Entities;
using Mps.OlivawCore.Api.Extensions.Entities;
using Mps.OlivawCore.Api.Helpers;
using Mps.OlivawCore.Domain;
using Mps.OlivawCore.Repositories;

namespace Mps.Limpo.Api.Features.ExampleFeature.Commands;

public class AlternarExampleCommand
    : IRequest<AlternarExampleCommandResponse>
{
    public Guid Id { get; set; }
    public bool Ativo { get; set; }

    internal Guid UsuarioLogadoId { get; set; }
    public AlternarExampleCommand() { }
    public AlternarExampleCommand
    (
        Guid usuarioLogadoId
    )
    {
        UsuarioLogadoId = usuarioLogadoId;
    }
}

public class AlternarExampleCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class AlternarExampleCommandHandler
(
    IRepository<Example> repository,
    AlternarExampleCommandValidator validator
) : IRequestHandler<AlternarExampleCommand,
    AlternarExampleCommandResponse>
{
    private readonly IRepository<Example> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public readonly AlternarExampleCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AlternarExampleCommandResponse> Handle
    (
        AlternarExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Example entity = await GetFirstAsync(request, cancellationToken);
        await UpdateAsync(request, entity, cancellationToken);

        return entity.Map<AlternarExampleCommandResponse>();
    }

    private async Task<Example> GetFirstAsync
    (
        AlternarExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            request.SearchPredicate(),
            cancellationToken
        );
    }

    private async Task UpdateAsync
    (
        AlternarExampleCommand request,
        Example entity,
        CancellationToken cancellationToken
    )
    {
        entity.Toggle(request.Ativo, request.UsuarioLogadoId);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class AlternarExampleCommandValidator
    : AbstractValidator<AlternarExampleCommand>
{
    public AlternarExampleCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AlternarExampleCommand, Guid>(item => item.Id));
    }
}