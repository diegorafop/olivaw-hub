using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.PerfilAcessoFeature.Commands;

public class RemoverPerfilAcessoCommand
    : IRequest<RemoverPerfilAcessoCommandResponse>
{
    public Guid Id { get; set; }
}

public class RemoverPerfilAcessoCommandResponse
{
    public Guid Id { get; set; }
}

public class RemoverPerfilAcessoCommandHandler
(
    IRepository<PerfilAcesso> repository
) : IRequestHandler<RemoverPerfilAcessoCommand,
    RemoverPerfilAcessoCommandResponse>
{
    private readonly IRepository<PerfilAcesso> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<RemoverPerfilAcessoCommandResponse> Handle
    (
        RemoverPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        PerfilAcesso entity = await GetFirstAsync
        (
            request,
            cancellationToken
        );

        await RemoveAsync(entity, cancellationToken);

        return entity.Map<RemoverPerfilAcessoCommandResponse>();
    }

    private async Task<PerfilAcesso> GetFirstAsync
    (
        RemoverPerfilAcessoCommand request,
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
        PerfilAcesso entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.RemoveAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class RemoverPerfilAcessoCommandValidator
    : AbstractValidator<RemoverPerfilAcessoCommand>
{
    public RemoverPerfilAcessoCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<RemoverPerfilAcessoCommand, Guid>
                (item => item.Id));
    }
}