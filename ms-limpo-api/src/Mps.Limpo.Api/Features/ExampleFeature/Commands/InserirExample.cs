using FluentValidation;
using MediatR;
using Mps.Limpo.Api.Extensions.Entities;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Extensions.Collections;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Domain;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Limpo.Api.Features.ExampleFeature.Commands;

public class InserirExampleCommand
    : IRequest<InserirExampleCommandResponse>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Permission { get; set; }

    internal Guid UsuarioLogadoId { get; set; }

    public InserirExampleCommand() { }
    public InserirExampleCommand
    (
        Guid usuarioLogadoId
    )
    {
        UsuarioLogadoId = usuarioLogadoId;
    }
}

public class InserirExampleCommandResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }

    public Guid UsuarioLogadoId { get; set; }
}

public class InserirExampleCommandHandler
(
    IRepository<Example> repository,
    InserirExampleCommandValidator validator
) : IRequestHandler<InserirExampleCommand,
    InserirExampleCommandResponse>
{
    private readonly IRepository<Example> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly InserirExampleCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<InserirExampleCommandResponse> Handle
    (
        InserirExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        await ExistsAsync(request, cancellationToken);
        Example entity = request.ToDomain();
        await AddAsync(entity, cancellationToken);

        return entity.Map<InserirExampleCommandResponse>();
    }

    private async Task ExistsAsync
    (
        InserirExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        bool exists = await _repository.ExistsAsync
        (
            SearchPredicate(request),
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            exists,
            cancellationToken
        );
    }

    private static Expression<Func<Example, bool>> SearchPredicate
    (
        InserirExampleCommand request
    )
    {
        Expression<Func<Example, bool>> predicate = item => true;

        if (!string.IsNullOrEmpty(request.Name))
            predicate = predicate.And(item => item
                .Name
                .ToUpper()
                .Equals(request
                    .Name
                    .ToUpper()));

        return predicate;
    }

    private async Task AddAsync
    (
        Example entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class InserirExampleCommandValidator
    : AbstractValidator<InserirExampleCommand>
{
    public InserirExampleCommandValidator()
    {
        RuleFor(item => item.Name)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirExampleCommand, string>(item => item.Name));

        RuleFor(item => item.Description)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirExampleCommand, string>(item => item.Description));
    }

    public async Task ValidateDataAsync
    (
        InserirExampleCommand request,
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
