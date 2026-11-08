using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Owh.OlivawCore.Domain;

namespace Olivaw.Identity.Repositories;

public interface IIdentityStore
{
    Task<T?> FindAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : Entity;
    Task<T?> FindIncludingRemovedAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : Entity;
    Task<IReadOnlyList<T>> ListAsync<T>(Expression<Func<T, bool>> predicate, int page, int size, CancellationToken cancellationToken) where T : Entity;
    Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : Entity;
    void Add<T>(T entity) where T : Entity;
    Task SaveAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> PermissionsAsync(int usuarioId, CancellationToken cancellationToken);
}
