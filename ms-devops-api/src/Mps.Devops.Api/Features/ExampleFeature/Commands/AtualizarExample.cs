using FluentValidation;
using MediatR;
using Mps.Devops.Api.Extensions.Entities;
using Mps.OlivawCore.Api.Extensions.Entities;
using Mps.OlivawCore.Api.Helpers;
using Mps.OlivawCore.Domain;
using Mps.OlivawCore.Repositories;

namespace Mps.Devops.Api.Features.ExampleFeature.Commands;

public class AtualizarExampleCommand
    : IRequest<AtualizarExampleCommandResponse>
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public bool Permission { get; set; }

    internal Guid UsuarioLogadoId { get; set; }

    public AtualizarExampleCommand() { }
    public AtualizarExampleCommand
    (
        Guid usuarioLogadoId
    )
    {
        UsuarioLogadoId = usuarioLogadoId;
    }
}

public class AtualizarExampleCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
}

public class AtualizarExampleCommandHandler
(
    IRepository<Example> repository,
    AtualizarExampleCommandValidator validator
) : IRequestHandler<AtualizarExampleCommand,
    AtualizarExampleCommandResponse>
{
    private readonly IRepository<Example> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly AtualizarExampleCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AtualizarExampleCommandResponse> Handle
    (
        AtualizarExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        await ExistsAsync(request, cancellationToken);
        Example entity = await GetFirstAsync(request, cancellationToken);
        await UpdateAsync(request, entity, cancellationToken);

        return entity.Map<AtualizarExampleCommandResponse>();
    }

    private async Task<Example> GetFirstAsync
    (
        AtualizarExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            request.SearchPredicateId(),
            cancellationToken
        );
    }

    private async Task ExistsAsync
    (
        AtualizarExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        bool exists = await _repository.ExistsAsync
        (
            request.SearchPredicateByName(),
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            exists,
            cancellationToken
        );
    }

    private async Task UpdateAsync
    (
        AtualizarExampleCommand request,
        Example entity,
        CancellationToken cancellationToken
    )
    {
        request.Update(entity);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class AtualizarExampleCommandValidator
    : AbstractValidator<AtualizarExampleCommand>
{
    public AtualizarExampleCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarExampleCommand, Guid>(item => item.Id));

        RuleFor(item => item.Name)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarExampleCommand, string>(item => item.Name));

        RuleFor(item => item.Description)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarExampleCommand, string>(item => item.Description));
    }

    public async Task ValidateDataAsync
    (
        AtualizarExampleCommand request,
        bool exists,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.Name)
            .Must(item => { return !exists; })
            .WithMessage(MessageHelper
                .DuplicateKeyFor(request, item => item.Name));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}