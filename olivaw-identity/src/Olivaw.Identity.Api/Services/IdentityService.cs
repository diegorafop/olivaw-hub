using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Mps.OlivawCore.Domain;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;

namespace Olivaw.Identity.Api.Services;

public sealed partial class IdentityService(IIdentityStore store, IPasswordHasher<Usuario> hasher)
{
    private readonly IIdentityStore _store = store;
    private readonly IPasswordHasher<Usuario> _hasher = hasher;

    public async Task<T> FindAsync<T>(Guid id, CancellationToken cancellationToken) where T : Entity
    {
        return await _store.FindAsync<T>(item => item.ExternalId == id, cancellationToken)
            ?? throw new IdentityFailure(404, "Registro não encontrado.");
    }

    public Task<IReadOnlyList<T>> ListAsync<T>(int page, int size, CancellationToken cancellationToken) where T : Entity
    {
        if (page < 1) throw new IdentityFailure(400, "Página deve ser positiva.");
        if (page > 1000000) throw new IdentityFailure(400, "Página excede o limite.");
        if (size < 1) throw new IdentityFailure(400, "Tamanho deve ser positivo.");
        if (size > 100) throw new IdentityFailure(400, "Tamanho máximo: 100.");
        return _store.ListAsync<T>(item => true, page, size, cancellationToken);
    }

    private async Task EnsureUniqueAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : Entity
    {
        if (await _store.ExistsAsync(predicate, cancellationToken))
            throw new IdentityFailure(409, "Identificador ou vínculo já cadastrado.");
    }

    private static void Created(Entity entity, Guid actor)
    {
        entity.ExternalId = Guid.NewGuid();
        entity.UsuarioLogadoId = actor;
        entity.CreatedAt = DateTime.UtcNow;
    }

    private static void Updated(Entity entity, Guid actor)
    {
        entity.UsuarioLogadoId = actor;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task RemoveAsync<T>(Guid id, Guid actor, CancellationToken cancellationToken) where T : Entity
    {
        var entity = await FindAsync<T>(id, cancellationToken);
        if (entity is Usuario usuario)
        {
            if (usuario.Root) throw new IdentityFailure(409, "O usuário root não pode ser removido.");
            usuario.VersaoSeguranca = Guid.NewGuid();
        }
        entity.DeletedAt = DateTime.UtcNow;
        Updated(entity, actor);
        await _store.SaveAsync(cancellationToken);
    }

    public async Task<Usuario> SaveUsuarioAsync(Guid? id, string login, string nome, string email, string? senha, bool ativo, Guid actor, CancellationToken cancellationToken)
    {
        Usuario entity;
        if (id.HasValue)
            entity = await FindAsync<Usuario>(id.Value, cancellationToken);
        else
        {
            entity = new Usuario();
            Created(entity, actor);
        }
        if (entity.Root)
        {
            if (!ativo) throw new IdentityFailure(409, "O usuário root não pode ser desativado.");
            if (login != "root") throw new IdentityFailure(409, "O login root é reservado.");
        }
        var normalized = login.Trim().ToUpperInvariant();
        if (!entity.Root)
        {
            if (normalized == "ROOT") throw new IdentityFailure(409, "O login root é reservado.");
        }
        await EnsureUniqueAsync<Usuario>(item => item.LoginNormalizado == normalized && item.ExternalId != entity.ExternalId, cancellationToken);
        entity.Login = login.Trim();
        entity.LoginNormalizado = normalized;
        entity.Nome = nome.Trim();
        entity.Email = email.Trim();
        if (entity.Ativo != ativo) entity.VersaoSeguranca = Guid.NewGuid();
        entity.Ativo = ativo;
        if (!id.HasValue)
        {
            if (string.IsNullOrEmpty(senha)) throw new IdentityFailure(400, "Senha obrigatória.");
            entity.SenhaHash = _hasher.HashPassword(entity, senha);
            _store.Add(entity);
        }
        Updated(entity, actor);
        await _store.SaveAsync(cancellationToken);
        return entity;
    }

    public async Task<PerfilAcesso> SavePerfilAcessoAsync(Guid? id, string nome, string? descricao, bool ativo, Guid actor, CancellationToken cancellationToken)
    {
        PerfilAcesso entity;
        if (id.HasValue) entity = await FindAsync<PerfilAcesso>(id.Value, cancellationToken);
        else
        {
            entity = new PerfilAcesso();
            Created(entity, actor);
        }
        var normalized = nome.Trim().ToUpperInvariant();
        await EnsureUniqueAsync<PerfilAcesso>(item => item.NomeNormalizado == normalized && item.ExternalId != entity.ExternalId, cancellationToken);
        entity.Nome = nome.Trim();
        entity.NomeNormalizado = normalized;
        entity.Descricao = descricao;
        entity.Ativo = ativo;
        Updated(entity, actor);
        if (!id.HasValue) _store.Add(entity);
        await _store.SaveAsync(cancellationToken);
        return entity;
    }

    public async Task<Autorizacao> SaveAutorizacaoAsync(Guid? id, string chave, string nome, string? descricao, Guid actor, CancellationToken cancellationToken)
    {
        Autorizacao entity;
        if (id.HasValue) entity = await FindAsync<Autorizacao>(id.Value, cancellationToken);
        else
        {
            entity = new Autorizacao();
            Created(entity, actor);
        }
        chave = chave.Trim().ToLowerInvariant();
        await EnsureUniqueAsync<Autorizacao>(item => item.Chave == chave && item.ExternalId != entity.ExternalId, cancellationToken);
        entity.Chave = chave;
        entity.Nome = nome.Trim();
        entity.Descricao = descricao;
        Updated(entity, actor);
        if (!id.HasValue) _store.Add(entity);
        await _store.SaveAsync(cancellationToken);
        return entity;
    }

    public async Task<UsuarioPerfilAcesso> SaveUsuarioPerfilAcessoAsync(Guid? id, Guid usuarioId, Guid perfilAcessoId, Guid actor, CancellationToken cancellationToken)
    {
        var usuario = await FindAsync<Usuario>(usuarioId, cancellationToken);
        var perfil = await FindAsync<PerfilAcesso>(perfilAcessoId, cancellationToken);
        UsuarioPerfilAcesso entity;
        if (id.HasValue) entity = await FindAsync<UsuarioPerfilAcesso>(id.Value, cancellationToken);
        else
        {
            var previous = await _store.FindIncludingRemovedAsync<UsuarioPerfilAcesso>(item => item.UsuarioId == usuario.Id && item.PerfilAcessoId == perfil.Id, cancellationToken);
            if (previous is not null)
            {
                if (previous.DeletedAt is null) throw new IdentityFailure(409, "Vínculo já cadastrado.");
                previous.DeletedAt = null;
                Updated(previous, actor);
                await _store.SaveAsync(cancellationToken);
                return previous;
            }
            entity = new UsuarioPerfilAcesso();
            Created(entity, actor);
        }
        await EnsureUniqueAsync<UsuarioPerfilAcesso>(item => item.UsuarioId == usuario.Id && item.PerfilAcessoId == perfil.Id && item.ExternalId != entity.ExternalId, cancellationToken);
        entity.Usuario = usuario;
        entity.UsuarioId = usuario.Id;
        entity.PerfilAcesso = perfil;
        entity.PerfilAcessoId = perfil.Id;
        Updated(entity, actor);
        if (!id.HasValue) _store.Add(entity);
        await _store.SaveAsync(cancellationToken);
        return entity;
    }

    public async Task<PerfilAcessoAutorizacao> SavePerfilAcessoAutorizacaoAsync(Guid? id, Guid perfilAcessoId, Guid autorizacaoId, Guid actor, CancellationToken cancellationToken)
    {
        var perfil = await FindAsync<PerfilAcesso>(perfilAcessoId, cancellationToken);
        var autorizacao = await FindAsync<Autorizacao>(autorizacaoId, cancellationToken);
        PerfilAcessoAutorizacao entity;
        if (id.HasValue) entity = await FindAsync<PerfilAcessoAutorizacao>(id.Value, cancellationToken);
        else
        {
            var previous = await _store.FindIncludingRemovedAsync<PerfilAcessoAutorizacao>(item => item.PerfilAcessoId == perfil.Id && item.AutorizacaoId == autorizacao.Id, cancellationToken);
            if (previous is not null)
            {
                if (previous.DeletedAt is null) throw new IdentityFailure(409, "Vínculo já cadastrado.");
                previous.DeletedAt = null;
                Updated(previous, actor);
                await _store.SaveAsync(cancellationToken);
                return previous;
            }
            entity = new PerfilAcessoAutorizacao();
            Created(entity, actor);
        }
        await EnsureUniqueAsync<PerfilAcessoAutorizacao>(item => item.PerfilAcessoId == perfil.Id && item.AutorizacaoId == autorizacao.Id && item.ExternalId != entity.ExternalId, cancellationToken);
        entity.PerfilAcesso = perfil;
        entity.PerfilAcessoId = perfil.Id;
        entity.Autorizacao = autorizacao;
        entity.AutorizacaoId = autorizacao.Id;
        Updated(entity, actor);
        if (!id.HasValue) _store.Add(entity);
        await _store.SaveAsync(cancellationToken);
        return entity;
    }
}
