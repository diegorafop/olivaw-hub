using FluentValidation;
using MediatR;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Commands;

public class RemoverUsuarioAutorizacaoCommand
    : IRequest<RemoverUsuarioAutorizacaoCommandResponse>
{
    public Guid Id { get; set; }
}

public class RemoverUsuarioAutorizacaoCommandResponse
{
    public Guid Id { get; set; }
}

public class RemoverUsuarioAutorizacaoCommandHandler
(
    IRepository<UsuarioAutorizacao> repository
) : IRequestHandler<RemoverUsuarioAutorizacaoCommand,
    RemoverUsuarioAutorizacaoCommandResponse>
{
    private readonly IRepository<UsuarioAutorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<RemoverUsuarioAutorizacaoCommandResponse> Handle
    (
        RemoverUsuarioAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        UsuarioAutorizacao entity = await GetFirstAsync(request, cancellationToken);
        await RemoveAsync(entity, cancellationToken);

        return entity.Map<RemoverUsuarioAutorizacaoCommandResponse>();
    }

    private async Task<UsuarioAutorizacao> GetFirstAsync
    (
        RemoverUsuarioAutorizacaoCommand request,
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
        UsuarioAutorizacao entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.RemoveAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class RemoverUsuarioAutorizacaoCommandValidator
    : AbstractValidator<RemoverUsuarioAutorizacaoCommand>
{
    public RemoverUsuarioAutorizacaoCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<RemoverUsuarioAutorizacaoCommand, Guid>
                (item => item.Id));
    }
}