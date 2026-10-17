using Mps.OlivawCore.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mps.OlivawCore.Repositories;

public interface IRepository<T> where T : Entity
{
    public Task<IEnumerable<T>> GetAsync
    (
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    );

    public Task<IEnumerable<T>> GetAsync
    (
        Expression<Func<T, bool>> lambda,
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    );

    public Task<IEnumerable<T>> TakeOrderedAsync
    (
        Expression<Func<T, bool>> lambda,
        int count,
        Expression<Func<T, object>> orderByDescending,
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    );

    public Task<T> GetFirstAsync
    (
        Expression<Func<T, bool>> lambda,
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    );

    public Task<T> GetFirstOrDefaultAsync
    (
        Expression<Func<T, bool>> lambda,
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    );

    public Task<T> GetOrderedFirstAsync
    (
        Expression<Func<T, bool>> lambda,
        Expression<Func<T, object>> orderByDescending,
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    );

    public Task<bool> ExistsAsync
    (
        Expression<Func<T, bool>> lambda,
        CancellationToken cancellationToken
    );

    public Task AddAsync
    (
        T entity,
        CancellationToken cancellationToken
    );

    public Task AddCollectionAsync
    (
        IEnumerable<T> entities,
        CancellationToken cancellationToken
    );

    public Task UpdateAsync
    (
        T entity,
        CancellationToken cancellationToken
    );

    public Task UpdateCollectionAsync
    (
        IEnumerable<T> entities,
        CancellationToken cancellationToken
    );

    public Task RemoveAsync
    (
        T entity,
        CancellationToken cancellationToken
    );

    public Task RemoveCollectionAsync
    (
        IEnumerable<T> entities,
        CancellationToken cancellationToken
    );

    public Task<int> SaveChangesAsync
    (
        CancellationToken cancellationToken
    );

    public Task<bool> CanConnectAsync
    (
        CancellationToken cancellationToken
    );
}