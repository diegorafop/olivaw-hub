using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Owh.OlivawCore.Domain;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;

namespace Olivaw.Identity.Api.Services;

public sealed partial class IdentityService(IIdentityStore store, IPasswordHasher<UserProfile> hasher)
{
    private readonly IIdentityStore _store = store;
    private readonly IPasswordHasher<UserProfile> _hasher = hasher;

    public async Task<T> FindAsync<T>(Guid id, CancellationToken cancellationToken) where T : Entity
    {
        return await _store.FindAsync<T>(item => item.ExternalId == id, cancellationToken)
            ?? throw new IdentityFailure(404, "Item não encontrado.");
    }

    public Task<IReadOnlyList<T>> ListAsync<T>(int page, int size, CancellationToken cancellationToken) where T : Entity
    {
        if (page < 1) throw new IdentityFailure(400, "Página deve ser positiva.");
        if (page > 1000000) throw new IdentityFailure(400, "Página excede o limite.");
        if (size < 1) throw new IdentityFailure(400, "PageSize deve ser positivo.");
        if (size > 100) throw new IdentityFailure(400, "PageSize máximo: 100.");
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
        entity.ActorUserId = actor;
        entity.CreatedAt = DateTime.UtcNow;
    }

    private static void Updated(Entity entity, Guid actor)
    {
        entity.ActorUserId = actor;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task RemoveAsync<T>(Guid id, Guid actor, CancellationToken cancellationToken) where T : Entity
    {
        var entity = await FindAsync<T>(id, cancellationToken);
        if (entity is UserProfile usuario)
        {
            if (usuario.IsRoot) throw new IdentityFailure(409, "O usuário root não pode ser removido.");
            usuario.SecurityStamp = Guid.NewGuid();
        }
        entity.DeletedAt = DateTime.UtcNow;
        Updated(entity, actor);
        await _store.SaveAsync(cancellationToken);
    }

    public async Task<UserProfile> SaveUserProfileAsync(Guid? id, string login, string nome, string email, string? senha, bool ativo, Guid actor, CancellationToken cancellationToken, bool isServiceAccount = false)
    {
        UserProfile entity;
        if (id.HasValue)
            entity = await FindAsync<UserProfile>(id.Value, cancellationToken);
        else
        {
            entity = new UserProfile();
            Created(entity, actor);
        }
        if (entity.IsRoot)
        {
            if (isServiceAccount) throw new IdentityFailure(409, "Root não pode ser uma conta técnica.");
            if (!ativo) throw new IdentityFailure(409, "O usuário root não pode ser desativado.");
            if (login != "root") throw new IdentityFailure(409, "O login root é reservado.");
        }
        var normalized = login.Trim().ToUpperInvariant();
        if (!entity.IsRoot)
        {
            if (normalized == "ROOT") throw new IdentityFailure(409, "O login root é reservado.");
        }
        await EnsureUniqueAsync<UserProfile>(item => item.NormalizedLogin == normalized && item.ExternalId != entity.ExternalId, cancellationToken);
        entity.Login = login.Trim();
        entity.NormalizedLogin = normalized;
        entity.Name = nome.Trim();
        entity.Email = email.Trim();
        if (entity.IsActive != ativo) entity.SecurityStamp = Guid.NewGuid();
        entity.IsServiceAccount = isServiceAccount;
        entity.IsActive = ativo;
        if (!id.HasValue)
        {
            if (string.IsNullOrEmpty(senha)) throw new IdentityFailure(400, "Password obrigatória.");
            entity.PasswordHash = _hasher.HashPassword(entity, senha);
            _store.Add(entity);
        }
        Updated(entity, actor);
        await _store.SaveAsync(cancellationToken);
        return entity;
    }

    public async Task<AccessProfile> SaveAccessProfileAsync(Guid? id, string nome, string? descricao, bool ativo, Guid actor, CancellationToken cancellationToken)
    {
        AccessProfile entity;
        if (id.HasValue) entity = await FindAsync<AccessProfile>(id.Value, cancellationToken);
        else
        {
            entity = new AccessProfile();
            Created(entity, actor);
        }
        var normalized = nome.Trim().ToUpperInvariant();
        await EnsureUniqueAsync<AccessProfile>(item => item.NormalizedName == normalized && item.ExternalId != entity.ExternalId, cancellationToken);
        entity.Name = nome.Trim();
        entity.NormalizedName = normalized;
        entity.Description = descricao;
        entity.IsActive = ativo;
        Updated(entity, actor);
        if (!id.HasValue) _store.Add(entity);
        await _store.SaveAsync(cancellationToken);
        return entity;
    }

    public async Task<Authorization> SaveAuthorizationAsync(Guid? id, string chave, string nome, string? descricao, Guid actor, CancellationToken cancellationToken)
    {
        Authorization entity;
        if (id.HasValue) entity = await FindAsync<Authorization>(id.Value, cancellationToken);
        else
        {
            entity = new Authorization();
            Created(entity, actor);
        }
        chave = chave.Trim().ToLowerInvariant();
        await EnsureUniqueAsync<Authorization>(item => item.Key == chave && item.ExternalId != entity.ExternalId, cancellationToken);
        entity.Key = chave;
        entity.Name = nome.Trim();
        entity.Description = descricao;
        Updated(entity, actor);
        if (!id.HasValue) _store.Add(entity);
        await _store.SaveAsync(cancellationToken);
        return entity;
    }

    public async Task<UserProfileAccessProfile> SaveUserProfileAccessProfileAsync(Guid? id, Guid usuarioId, Guid perfilAcessoId, Guid actor, CancellationToken cancellationToken)
    {
        var usuario = await FindAsync<UserProfile>(usuarioId, cancellationToken);
        var perfil = await FindAsync<AccessProfile>(perfilAcessoId, cancellationToken);
        UserProfileAccessProfile entity;
        if (id.HasValue) entity = await FindAsync<UserProfileAccessProfile>(id.Value, cancellationToken);
        else
        {
            var previous = await _store.FindIncludingRemovedAsync<UserProfileAccessProfile>(item => item.UserProfileId == usuario.Id && item.AccessProfileId == perfil.Id, cancellationToken);
            if (previous is not null)
            {
                if (previous.DeletedAt is null) throw new IdentityFailure(409, "Vínculo já cadastrado.");
                previous.DeletedAt = null;
                Updated(previous, actor);
                await _store.SaveAsync(cancellationToken);
                return previous;
            }
            entity = new UserProfileAccessProfile();
            Created(entity, actor);
        }
        await EnsureUniqueAsync<UserProfileAccessProfile>(item => item.UserProfileId == usuario.Id && item.AccessProfileId == perfil.Id && item.ExternalId != entity.ExternalId, cancellationToken);
        entity.UserProfile = usuario;
        entity.UserProfileId = usuario.Id;
        entity.AccessProfile = perfil;
        entity.AccessProfileId = perfil.Id;
        Updated(entity, actor);
        if (!id.HasValue) _store.Add(entity);
        await _store.SaveAsync(cancellationToken);
        return entity;
    }

    public async Task<AccessProfileAuthorization> SaveAccessProfileAuthorizationAsync(Guid? id, Guid perfilAcessoId, Guid autorizacaoId, Guid actor, CancellationToken cancellationToken)
    {
        var perfil = await FindAsync<AccessProfile>(perfilAcessoId, cancellationToken);
        var autorizacao = await FindAsync<Authorization>(autorizacaoId, cancellationToken);
        AccessProfileAuthorization entity;
        if (id.HasValue) entity = await FindAsync<AccessProfileAuthorization>(id.Value, cancellationToken);
        else
        {
            var previous = await _store.FindIncludingRemovedAsync<AccessProfileAuthorization>(item => item.AccessProfileId == perfil.Id && item.AuthorizationId == autorizacao.Id, cancellationToken);
            if (previous is not null)
            {
                if (previous.DeletedAt is null) throw new IdentityFailure(409, "Vínculo já cadastrado.");
                previous.DeletedAt = null;
                Updated(previous, actor);
                await _store.SaveAsync(cancellationToken);
                return previous;
            }
            entity = new AccessProfileAuthorization();
            Created(entity, actor);
        }
        await EnsureUniqueAsync<AccessProfileAuthorization>(item => item.AccessProfileId == perfil.Id && item.AuthorizationId == autorizacao.Id && item.ExternalId != entity.ExternalId, cancellationToken);
        entity.AccessProfile = perfil;
        entity.AccessProfileId = perfil.Id;
        entity.Authorization = autorizacao;
        entity.AuthorizationId = autorizacao.Id;
        Updated(entity, actor);
        if (!id.HasValue) _store.Add(entity);
        await _store.SaveAsync(cancellationToken);
        return entity;
    }
}
