using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AutorizacaoFeature.Queries;

public sealed class SelecionarAutorizacaoByFilterQuery : IRequest<SelecionarAutorizacaoByFilterResponse>
{
    public int Pagina { get; set; } = 1;
    public int Tamanho { get; set; } = 20;
}
public sealed class SelecionarAutorizacaoByFilterResponse
{
    public IReadOnlyList<AutorizacaoResponse> Registros { get; set; } = Array.Empty<AutorizacaoResponse>();
}
public sealed class SelecionarAutorizacaoByFilterValidator : AbstractValidator<SelecionarAutorizacaoByFilterQuery>
{
    public SelecionarAutorizacaoByFilterValidator()
    {
        RuleFor(item => item.Pagina).InclusiveBetween(1, 1000000);
        RuleFor(item => item.Tamanho).InclusiveBetween(1, 100);
    }
}
public sealed class SelecionarAutorizacaoByFilterHandler(IdentityService service)
    : IRequestHandler<SelecionarAutorizacaoByFilterQuery, SelecionarAutorizacaoByFilterResponse>
{
    public async Task<SelecionarAutorizacaoByFilterResponse> Handle(SelecionarAutorizacaoByFilterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new SelecionarAutorizacaoByFilterValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entities = await service.ListAsync<Autorizacao>(request.Pagina, request.Tamanho, cancellationToken);
        var responses = new List<AutorizacaoResponse>();
        foreach (var entity in entities)
            responses.Add(AutorizacaoResponse.From(entity));
        return new SelecionarAutorizacaoByFilterResponse { Registros = responses };
    }
}
