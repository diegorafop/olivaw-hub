using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;

public sealed class SelecionarPerfilAcessoAutorizacaoByFilterQuery : IRequest<SelecionarPerfilAcessoAutorizacaoByFilterResponse>
{
    public int Pagina { get; set; } = 1;
    public int Tamanho { get; set; } = 20;
}
public sealed class SelecionarPerfilAcessoAutorizacaoByFilterResponse
{
    public IReadOnlyList<PerfilAcessoAutorizacaoResponse> Registros { get; set; } = Array.Empty<PerfilAcessoAutorizacaoResponse>();
}
public sealed class SelecionarPerfilAcessoAutorizacaoByFilterValidator : AbstractValidator<SelecionarPerfilAcessoAutorizacaoByFilterQuery>
{
    public SelecionarPerfilAcessoAutorizacaoByFilterValidator()
    {
        RuleFor(item => item.Pagina).InclusiveBetween(1, 1000000);
        RuleFor(item => item.Tamanho).InclusiveBetween(1, 100);
    }
}
public sealed class SelecionarPerfilAcessoAutorizacaoByFilterHandler(IdentityService service)
    : IRequestHandler<SelecionarPerfilAcessoAutorizacaoByFilterQuery, SelecionarPerfilAcessoAutorizacaoByFilterResponse>
{
    public async Task<SelecionarPerfilAcessoAutorizacaoByFilterResponse> Handle(SelecionarPerfilAcessoAutorizacaoByFilterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new SelecionarPerfilAcessoAutorizacaoByFilterValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entities = await service.ListAsync<PerfilAcessoAutorizacao>(request.Pagina, request.Tamanho, cancellationToken);
        var responses = new List<PerfilAcessoAutorizacaoResponse>();
        foreach (var entity in entities)
            responses.Add(PerfilAcessoAutorizacaoResponse.From(entity));
        return new SelecionarPerfilAcessoAutorizacaoByFilterResponse { Registros = responses };
    }
}
