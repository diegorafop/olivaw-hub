using FluentValidation;
using MediatR;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Commands;

public class RemoverUsuarioPerfilAcessoCommand
    : IRequest<RemoverUsuarioPerfilAcessoCommandResponse>
{
    public Guid Id { get; set; }
}

public class RemoverUsuarioPerfilAcessoCommandResponse
{
    public Guid Id { get; set; }
}

public class RemoverUsuarioPerfilAcessoCommandHandler
(
    IRepository<UsuarioPerfilAcesso> repository,
    RemoverUsuarioPerfilAcessoCommandValidator validator
) : IRequestHandler<RemoverUsuarioPerfilAcessoCommand,
    RemoverUsuarioPerfilAcessoCommandResponse>
{
    private readonly IRepository<UsuarioPerfilAcesso> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly RemoverUsuarioPerfilAcessoCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<RemoverUsuarioPerfilAcessoCommandResponse> Handle
    (
        RemoverUsuarioPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Domain.UsuarioPerfilAcesso entity = await GetFirstAsync(request, cancellationToken);
        await RemoveAsync(entity, cancellationToken);

        return entity.Map<RemoverUsuarioPerfilAcessoCommandResponse>();
    }

    private async Task<UsuarioPerfilAcesso> GetFirstAsync
    (
        RemoverUsuarioPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        UsuarioPerfilAcesso entity = await _repository.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.Id), cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            entity,
            cancellationToken
        );

        return entity;
    }

    private async Task RemoveAsync
    (
        UsuarioPerfilAcesso entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.RemoveAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class RemoverUsuarioPerfilAcessoCommandValidator
    : AbstractValidator<RemoverUsuarioPerfilAcessoCommand>
{
    public RemoverUsuarioPerfilAcessoCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<RemoverUsuarioPerfilAcessoCommand, Guid>
                (item => item.Id));
    }

    public async Task ValidateDataAsync
    (
        RemoverUsuarioPerfilAcessoCommand request,
        UsuarioPerfilAcesso entity,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.Id)
            .Must(item => { return entity != null; })
            .WithMessage(MessageHelper
                .NotFoundFor(request, item => item.Id));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}