using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;

public sealed class SelecionarPerfilAcessoAutorizacaoByIdQuery : IRequest<SelecionarPerfilAcessoAutorizacaoByIdResponse>
{
    public Guid Id { get; set; }
}
public sealed class SelecionarPerfilAcessoAutorizacaoByIdResponse
{
    public PerfilAcessoAutorizacaoResponse Registro { get; set; } = new();
}
public sealed class SelecionarPerfilAcessoAutorizacaoByIdValidator : AbstractValidator<SelecionarPerfilAcessoAutorizacaoByIdQuery>
{
    public SelecionarPerfilAcessoAutorizacaoByIdValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class SelecionarPerfilAcessoAutorizacaoByIdHandler(IdentityService service)
    : IRequestHandler<SelecionarPerfilAcessoAutorizacaoByIdQuery, SelecionarPerfilAcessoAutorizacaoByIdResponse>
{
    public async Task<SelecionarPerfilAcessoAutorizacaoByIdResponse> Handle(SelecionarPerfilAcessoAutorizacaoByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new SelecionarPerfilAcessoAutorizacaoByIdValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.FindAsync<PerfilAcessoAutorizacao>(request.Id, cancellationToken);
        return new SelecionarPerfilAcessoAutorizacaoByIdResponse { Registro = PerfilAcessoAutorizacaoResponse.From(entity) };
    }
}
