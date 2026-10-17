using FluentValidation;
using MediatR;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class RemoverUsuarioCommand
    : IRequest<RemoverUsuarioCommandResponse>
{
    public Guid Id { get; set; }
}

public class RemoverUsuarioCommandResponse
{
    public Guid Id { get; set; }
}

public class RemoverUsuarioCommandHandler
(
    IRepositoryUsuario repository
) : IRequestHandler<RemoverUsuarioCommand,
    RemoverUsuarioCommandResponse>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<RemoverUsuarioCommandResponse> Handle
    (
        RemoverUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Usuario entity = await GetFirstAsync
        (
            request,
            cancellationToken
        );

        await RemoveAsync
        (
            entity,
            cancellationToken
        );

        return entity.Map<RemoverUsuarioCommandResponse>();
    }

    private async Task RemoveAsync
    (
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.RemoveAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Usuario> GetFirstAsync
    (
        RemoverUsuarioCommand request,
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

public class RemoverUsuarioCommandValidator
    : AbstractValidator<RemoverUsuarioCommand>
{
    public RemoverUsuarioCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<RemoverUsuarioCommand, Guid>
                (item => item.Id));
    }
}