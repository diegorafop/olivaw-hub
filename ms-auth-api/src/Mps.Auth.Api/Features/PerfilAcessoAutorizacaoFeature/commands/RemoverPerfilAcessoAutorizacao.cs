using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;

public class RemoverPerfilAcessoAutorizacaoCommand
    : IRequest<RemoverPerfilAcessoAutorizacaoCommandResponse>
{
    public Guid Id { get; set; }
}

public class RemoverPerfilAcessoAutorizacaoCommandResponse
{
    public Guid Id { get; set; }
}

public class RemoverPerfilAcessoAutorizacaoCommandHandler
(
    IRepository<PerfilAcessoAutorizacao> repository,
    RemoverPerfilAcessoAutorizacaoCommandValidator validator
) : IRequestHandler<RemoverPerfilAcessoAutorizacaoCommand,
    RemoverPerfilAcessoAutorizacaoCommandResponse>
{
    private readonly IRepository<PerfilAcessoAutorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly RemoverPerfilAcessoAutorizacaoCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<RemoverPerfilAcessoAutorizacaoCommandResponse> Handle
    (
        RemoverPerfilAcessoAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        PerfilAcessoAutorizacao entity = await GetFirstAsync
        (
            request,
            cancellationToken
        );

        await RemoveAutorizacaoPerfilAcessoAsync(entity, cancellationToken);

        return entity.Map<RemoverPerfilAcessoAutorizacaoCommandResponse>();
    }

    private async Task<PerfilAcessoAutorizacao> GetFirstAsync
    (
        RemoverPerfilAcessoAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        PerfilAcessoAutorizacao entity = await _repository.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.Id),
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            entity,
            cancellationToken
        );

        return entity;
    }

    private async Task RemoveAutorizacaoPerfilAcessoAsync
    (
        PerfilAcessoAutorizacao entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.RemoveAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class RemoverPerfilAcessoAutorizacaoCommandValidator
    : AbstractValidator<RemoverPerfilAcessoAutorizacaoCommand>
{
    public RemoverPerfilAcessoAutorizacaoCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<RemoverPerfilAcessoAutorizacaoCommand, Guid>
                (item => item.Id));
    }

    public async Task ValidateDataAsync
    (
        RemoverPerfilAcessoAutorizacaoCommand request,
        PerfilAcessoAutorizacao entity,
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