using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Mps.OlivawCore.Domain;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;

namespace Olivaw.Identity.Dados;

public sealed class IdentityStore<TContext>(TContext context) : IIdentityStore where TContext : DbContext
{
    private IQueryable<T> Query<T>() where T : Entity
    {
        var query = context.Set<T>().AsQueryable();
        if (typeof(T) == typeof(UsuarioPerfilAcesso))
            return query.Include(nameof(UsuarioPerfilAcesso.Usuario)).Include(nameof(UsuarioPerfilAcesso.PerfilAcesso));
        if (typeof(T) == typeof(PerfilAcessoAutorizacao))
            return query.Include(nameof(PerfilAcessoAutorizacao.PerfilAcesso)).Include(nameof(PerfilAcessoAutorizacao.Autorizacao));
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
        var query = from usuario in context.Set<Usuario>().Where(item => item.Ativo)
                    join vinculo in context.Set<UsuarioPerfilAcesso>() on usuario.Id equals vinculo.UsuarioId
                    join perfil in context.Set<PerfilAcesso>().Where(item => item.Ativo) on vinculo.PerfilAcessoId equals perfil.Id
                    join permissao in context.Set<PerfilAcessoAutorizacao>() on perfil.Id equals permissao.PerfilAcessoId
                    join autorizacao in context.Set<Autorizacao>() on permissao.AutorizacaoId equals autorizacao.Id
                    where usuario.Id == usuarioId
                    select autorizacao.Chave;
        return await query.Distinct().ToListAsync(cancellationToken);
    }
}
