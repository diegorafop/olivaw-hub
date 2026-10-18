using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Owh.OlivawCore.Domain;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;

namespace Olivaw.Identity.Dados;

public sealed class IdentityStore<TContext>(TContext context) : IIdentityStore where TContext : DbContext
{
    private IQueryable<T> Query<T>() where T : Entity
    {
        var query = context.Set<T>().AsQueryable();
        if (typeof(T) == typeof(UserProfileAccessProfile))
            return query.Include(nameof(UserProfileAccessProfile.UserProfile)).Include(nameof(UserProfileAccessProfile.AccessProfile));
        if (typeof(T) == typeof(AccessProfileAuthorization))
            return query.Include(nameof(AccessProfileAuthorization.AccessProfile)).Include(nameof(AccessProfileAuthorization.Authorization));
        return query;
    }

    public Task<T?> FindAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : Entity
    {
        return Query<T>().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<T?> FindIncludingRemovedAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : Entity
    {
        return Query<T>().IgnoreQueryFilters().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> ListAsync<T>(Expression<Func<T, bool>> predicate, int page, int size, CancellationToken cancellationToken) where T : Entity
    {
        return await Query<T>().AsNoTracking().Where(predicate).OrderBy(item => item.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : Entity
    {
        return context.Set<T>().IgnoreQueryFilters().AnyAsync(predicate, cancellationToken);
    }

    public void Add<T>(T entity) where T : Entity
    {
        context.Set<T>().Add(entity);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> PermissionsAsync(int usuarioId, CancellationToken cancellationToken)
    {
        var query = from usuario in context.Set<UserProfile>().Where(item => item.IsActive)
                    join vinculo in context.Set<UserProfileAccessProfile>() on usuario.Id equals vinculo.UserProfileId
                    join perfil in context.Set<AccessProfile>().Where(item => item.IsActive) on vinculo.AccessProfileId equals perfil.Id
                    join permissao in context.Set<AccessProfileAuthorization>() on perfil.Id equals permissao.AccessProfileId
                    join autorizacao in context.Set<Authorization>() on permissao.AuthorizationId equals autorizacao.Id
                    where usuario.Id == usuarioId
                    select autorizacao.Key;
        return await query.Distinct().ToListAsync(cancellationToken);
    }
}
