using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.AutorizacaoFeature.Commands;

public class RemoverAutorizacaoCommand
    : IRequest<RemoverAutorizacaoCommandResponse>
{
    public Guid Id { get; set; }
}

public class RemoverAutorizacaoCommandResponse
{
    public Guid Id { get; set; }
}

public class RemoverAutorizacaoCommandHandler
(
    IRepository<Autorizacao> repository
) : IRequestHandler<RemoverAutorizacaoCommand,
    RemoverAutorizacaoCommandResponse>
{
    private readonly IRepository<Autorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<RemoverAutorizacaoCommandResponse> Handle
    (
        RemoverAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Autorizacao entity = await GetFirstAsync(request, cancellationToken);
        await RemoveAsync(entity, cancellationToken);

        return entity.Map<RemoverAutorizacaoCommandResponse>();
    }

    private async Task<Autorizacao> GetFirstAsync
    (
        RemoverAutorizacaoCommand request,
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

    private async Task RemoveAsync
    (
        Autorizacao entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.RemoveAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class RemoverAutorizacaoCommandValidator
    : AbstractValidator<RemoverAutorizacaoCommand>
{
    public RemoverAutorizacaoCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<RemoverAutorizacaoCommand, Guid>
                (item => item.Id));
    }
}