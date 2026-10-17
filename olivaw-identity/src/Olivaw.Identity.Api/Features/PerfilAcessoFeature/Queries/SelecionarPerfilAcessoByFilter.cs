using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.PerfilAcessoFeature.Queries;

public sealed class SelecionarPerfilAcessoByFilterQuery : IRequest<SelecionarPerfilAcessoByFilterResponse>
{
    public int Pagina { get; set; } = 1;
    public int Tamanho { get; set; } = 20;
}
public sealed class SelecionarPerfilAcessoByFilterResponse
{
    public IReadOnlyList<PerfilAcessoResponse> Registros { get; set; } = Array.Empty<PerfilAcessoResponse>();
}
public sealed class SelecionarPerfilAcessoByFilterValidator : AbstractValidator<SelecionarPerfilAcessoByFilterQuery>
{
    public SelecionarPerfilAcessoByFilterValidator()
    {
        RuleFor(item => item.Pagina).InclusiveBetween(1, 1000000);
        RuleFor(item => item.Tamanho).InclusiveBetween(1, 100);
    }
}
public sealed class SelecionarPerfilAcessoByFilterHandler(IdentityService service)
    : IRequestHandler<SelecionarPerfilAcessoByFilterQuery, SelecionarPerfilAcessoByFilterResponse>
{
    public async Task<SelecionarPerfilAcessoByFilterResponse> Handle(SelecionarPerfilAcessoByFilterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new SelecionarPerfilAcessoByFilterValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entities = await service.ListAsync<PerfilAcesso>(request.Pagina, request.Tamanho, cancellationToken);
        var responses = new List<PerfilAcessoResponse>();
        foreach (var entity in entities)
            responses.Add(PerfilAcessoResponse.From(entity));
        return new SelecionarPerfilAcessoByFilterResponse { Registros = responses };
    }
}
