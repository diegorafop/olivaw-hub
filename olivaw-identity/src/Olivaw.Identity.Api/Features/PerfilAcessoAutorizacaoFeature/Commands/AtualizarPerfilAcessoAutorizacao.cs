using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;

public sealed class AtualizarPerfilAcessoAutorizacaoCommand : IRequest<AtualizarPerfilAcessoAutorizacaoResponse>
{
    public Guid Id { get; set; }
    public Guid PerfilAcessoId { get; set; }
    public Guid AutorizacaoId { get; set; }
}

public sealed class AtualizarPerfilAcessoAutorizacaoResponse
{
    public PerfilAcessoAutorizacaoResponse Registro { get; set; } = new();
}

public sealed class AtualizarPerfilAcessoAutorizacaoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<AtualizarPerfilAcessoAutorizacaoCommand, AtualizarPerfilAcessoAutorizacaoResponse>
{
    public async Task<AtualizarPerfilAcessoAutorizacaoResponse> Handle(AtualizarPerfilAcessoAutorizacaoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new AtualizarPerfilAcessoAutorizacaoValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SavePerfilAcessoAutorizacaoAsync(request.Id, request.PerfilAcessoId, request.AutorizacaoId, actor.Id, cancellationToken);
        return new AtualizarPerfilAcessoAutorizacaoResponse { Registro = PerfilAcessoAutorizacaoResponse.From(entity) };
    }
}

public sealed class AtualizarPerfilAcessoAutorizacaoValidator : AbstractValidator<AtualizarPerfilAcessoAutorizacaoCommand>
{
    public AtualizarPerfilAcessoAutorizacaoValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.PerfilAcessoId).NotEmpty();
        RuleFor(item => item.AutorizacaoId).NotEmpty();
    }
}
