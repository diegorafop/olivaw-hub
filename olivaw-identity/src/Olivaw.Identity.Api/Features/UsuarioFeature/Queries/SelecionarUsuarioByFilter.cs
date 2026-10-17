using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UsuarioFeature.Queries;

public sealed class SelecionarUsuarioByFilterQuery : IRequest<SelecionarUsuarioByFilterResponse>
{
    public int Pagina { get; set; } = 1;
    public int Tamanho { get; set; } = 20;
}
public sealed class SelecionarUsuarioByFilterResponse
{
    public IReadOnlyList<UsuarioResponse> Registros { get; set; } = Array.Empty<UsuarioResponse>();
}
public sealed class SelecionarUsuarioByFilterValidator : AbstractValidator<SelecionarUsuarioByFilterQuery>
{
    public SelecionarUsuarioByFilterValidator()
    {
        RuleFor(item => item.Pagina).InclusiveBetween(1, 1000000);
        RuleFor(item => item.Tamanho).InclusiveBetween(1, 100);
    }
}
public sealed class SelecionarUsuarioByFilterHandler(IdentityService service)
    : IRequestHandler<SelecionarUsuarioByFilterQuery, SelecionarUsuarioByFilterResponse>
{
    public async Task<SelecionarUsuarioByFilterResponse> Handle(SelecionarUsuarioByFilterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new SelecionarUsuarioByFilterValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entities = await service.ListAsync<Usuario>(request.Pagina, request.Tamanho, cancellationToken);
        var responses = new List<UsuarioResponse>();
        foreach (var entity in entities)
            responses.Add(UsuarioResponse.From(entity));
        return new SelecionarUsuarioByFilterResponse { Registros = responses };
    }
}
