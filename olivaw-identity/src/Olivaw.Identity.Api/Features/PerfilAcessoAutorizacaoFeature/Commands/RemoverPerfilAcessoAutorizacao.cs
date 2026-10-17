using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;

public sealed class RemoverPerfilAcessoAutorizacaoCommand : IRequest<RemoverPerfilAcessoAutorizacaoResponse>
{
    public Guid Id { get; set; }
}
public sealed class RemoverPerfilAcessoAutorizacaoResponse { }
public sealed class RemoverPerfilAcessoAutorizacaoValidator : AbstractValidator<RemoverPerfilAcessoAutorizacaoCommand>
{
    public RemoverPerfilAcessoAutorizacaoValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class RemoverPerfilAcessoAutorizacaoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<RemoverPerfilAcessoAutorizacaoCommand, RemoverPerfilAcessoAutorizacaoResponse>
{
    public async Task<RemoverPerfilAcessoAutorizacaoResponse> Handle(RemoverPerfilAcessoAutorizacaoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new RemoverPerfilAcessoAutorizacaoValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.RemoveAsync<PerfilAcessoAutorizacao>(request.Id, actor.Id, cancellationToken);
        return new RemoverPerfilAcessoAutorizacaoResponse();
    }
}
