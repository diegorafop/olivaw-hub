using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;

public sealed class InserirPerfilAcessoAutorizacaoCommand : IRequest<InserirPerfilAcessoAutorizacaoResponse>
{
    public Guid PerfilAcessoId { get; set; }
    public Guid AutorizacaoId { get; set; }
}

public sealed class InserirPerfilAcessoAutorizacaoResponse
{
    public PerfilAcessoAutorizacaoResponse Registro { get; set; } = new();
}

public sealed class InserirPerfilAcessoAutorizacaoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<InserirPerfilAcessoAutorizacaoCommand, InserirPerfilAcessoAutorizacaoResponse>
{
    public async Task<InserirPerfilAcessoAutorizacaoResponse> Handle(InserirPerfilAcessoAutorizacaoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new InserirPerfilAcessoAutorizacaoValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SavePerfilAcessoAutorizacaoAsync(null, request.PerfilAcessoId, request.AutorizacaoId, actor.Id, cancellationToken);
        return new InserirPerfilAcessoAutorizacaoResponse { Registro = PerfilAcessoAutorizacaoResponse.From(entity) };
    }
}

public sealed class InserirPerfilAcessoAutorizacaoValidator : AbstractValidator<InserirPerfilAcessoAutorizacaoCommand>
{
    public InserirPerfilAcessoAutorizacaoValidator()
    {
        RuleFor(item => item.PerfilAcessoId).NotEmpty();
        RuleFor(item => item.AutorizacaoId).NotEmpty();
    }
}
