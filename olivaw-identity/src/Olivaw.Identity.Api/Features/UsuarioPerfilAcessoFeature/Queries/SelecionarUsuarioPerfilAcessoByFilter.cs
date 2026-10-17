using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature.Queries;

public sealed class SelecionarUsuarioPerfilAcessoByFilterQuery : IRequest<SelecionarUsuarioPerfilAcessoByFilterResponse>
{
    public int Pagina { get; set; } = 1;
    public int Tamanho { get; set; } = 20;
}
public sealed class SelecionarUsuarioPerfilAcessoByFilterResponse
{
    public IReadOnlyList<UsuarioPerfilAcessoResponse> Registros { get; set; } = Array.Empty<UsuarioPerfilAcessoResponse>();
}
public sealed class SelecionarUsuarioPerfilAcessoByFilterValidator : AbstractValidator<SelecionarUsuarioPerfilAcessoByFilterQuery>
{
    public SelecionarUsuarioPerfilAcessoByFilterValidator()
    {
        RuleFor(item => item.Pagina).InclusiveBetween(1, 1000000);
        RuleFor(item => item.Tamanho).InclusiveBetween(1, 100);
    }
}
public sealed class SelecionarUsuarioPerfilAcessoByFilterHandler(IdentityService service)
    : IRequestHandler<SelecionarUsuarioPerfilAcessoByFilterQuery, SelecionarUsuarioPerfilAcessoByFilterResponse>
{
    public async Task<SelecionarUsuarioPerfilAcessoByFilterResponse> Handle(SelecionarUsuarioPerfilAcessoByFilterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new SelecionarUsuarioPerfilAcessoByFilterValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entities = await service.ListAsync<UsuarioPerfilAcesso>(request.Pagina, request.Tamanho, cancellationToken);
        var responses = new List<UsuarioPerfilAcessoResponse>();
        foreach (var entity in entities)
            responses.Add(UsuarioPerfilAcessoResponse.From(entity));
        return new SelecionarUsuarioPerfilAcessoByFilterResponse { Registros = responses };
    }
}
