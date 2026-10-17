using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;

namespace Olivaw.Identity.Api.Features.AutenticacaoFeature.Queries;

public sealed class SelecionarAcessoAtualQuery : IRequest<SelecionarAcessoAtualResponse> { }

public sealed class SelecionarAcessoAtualResponse
{
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Root { get; set; }
    public IReadOnlyList<string> Autorizacoes { get; set; } = Array.Empty<string>();
}

public sealed class SelecionarAcessoAtualHandler(IIdentityStore store, CurrentActor actor)
    : IRequestHandler<SelecionarAcessoAtualQuery, SelecionarAcessoAtualResponse>
{
    public async Task<SelecionarAcessoAtualResponse> Handle(SelecionarAcessoAtualQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var id = actor.Id;
        var usuario = await store.FindAsync<Usuario>(item => item.IdExterno == id, cancellationToken)
            ?? throw new IdentityFailure(401, "Autenticação obrigatória.");
        return new SelecionarAcessoAtualResponse
        {
            UsuarioId = usuario.IdExterno,
            Nome = usuario.Nome,
            Root = usuario.Root,
            Autorizacoes = await store.PermissionsAsync(usuario.Id, cancellationToken)
        };
    }
}
