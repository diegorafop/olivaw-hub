using FluentValidation;
using MediatR;
using Mps.Limpo.Api.Extensions.Entities;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Domain;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Limpo.Api.Features.ExampleFeature.Commands;

public class RemoverExampleCommand
    : IRequest<RemoverExampleCommandResponse>
{
    public Guid Id { get; set; }
}

public class RemoverExampleCommandResponse
{
    public Guid Id { get; set; }
}

public class RemoverExampleCommandHandler
(
    IRepository<Example> repository
) : IRequestHandler<RemoverExampleCommand,
    RemoverExampleCommandResponse>
{
    private readonly IRepository<Example> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<RemoverExampleCommandResponse> Handle
    (
        RemoverExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Example entity = await GetFirstAsync
        (
            request,
            cancellationToken
        );

        await RemoveAsync
        (
            entity,
            cancellationToken
        );

        return entity.Map<RemoverExampleCommandResponse>();
    }

    private async Task<Example> GetFirstAsync
    (
        RemoverExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            SearchPredicate(request),
            cancellationToken
        );
    }

    private static Expression<Func<Example, bool>> SearchPredicate
    (
        RemoverExampleCommand request
    )
    {
        return item => item
            .IdExterno
            .Equals(request.Id);
    }

    private async Task RemoveAsync
    (
        Example entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.RemoveAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class RemoverExampleCommandValidator
    : AbstractValidator<RemoverExampleCommand>
{
    public RemoverExampleCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<RemoverExampleCommand, Guid>(item => item.Id));
    }
}