using Microsoft.EntityFrameworkCore;
using Owh.OlivawCore.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Dados.Repositories;
using System.Linq.Expressions;

namespace Owh.Olivaw.Dados.Repositories;

public class Repository<T>
(
    WriteDbContext context
) : IRepository<T> where T : Entity
{
    protected readonly IWriteDbContext _context = context;
    protected readonly EntityValidator<T> _validator = new();

    protected IQueryable<T> Query
    (
        params Expression<Func<T, object>>[] joins
    )
    {
        var query = _context
            .Set<T>()
            .AsQueryable();

        if (joins == null)
            return query;

        return joins.Aggregate(query, GetExpression());
    }

    protected IQueryable<T> Query
    (
        Expression<Func<T, object>> orderByDescending,
        params Expression<Func<T, object>>[] joins
    )
    {
        var query = _context
            .Set<T>()
            .OrderByDescending(orderByDescending)
            .AsQueryable();

        if (joins == null)
            return query;

        return joins.Aggregate(query, GetExpression());
    }

    public virtual async Task<IEnumerable<T>> GetAsync
    (
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    )
    {
        return await Query(joins).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAsync
    (
        Expression<Func<T, bool>> lambda,
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    )
    {
        return await Query(joins)
            .Where(lambda)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> TakeOrderedAsync
    (
        Expression<Func<T, bool>> lambda,
        int count,
        Expression<Func<T, object>> orderByDescending,
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    )
    {
        return await Query(orderByDescending, joins)
            .Where(lambda)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<T> GetFirstAsync
    (
        Expression<Func<T, bool>> lambda,
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    )
    {
        var entity = await GetFirstOrDefaultAsync(lambda, cancellationToken);
        await _validator.NotFoundAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task<T> GetFirstOrDefaultAsync
    (
        Expression<Func<T, bool>> lambda,
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    )
    {
        return await Query(joins).FirstOrDefaultAsync(lambda, cancellationToken);
    }

    public virtual async Task<T> GetOrderedFirstAsync
    (
        Expression<Func<T, bool>> lambda,
        Expression<Func<T, object>> orderByDescending,
        CancellationToken cancellationToken,
        params Expression<Func<T, object>>[] joins
    )
    {
        var entity = await Query(orderByDescending, joins).FirstOrDefaultAsync();
        await _validator.NotFoundAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task<bool> ExistsAsync
    (
        Expression<Func<T, bool>> lambda,
        CancellationToken cancellationToken
    )
    {
        return await Query().AnyAsync(lambda, cancellationToken);
    }

    public virtual async Task AddAsync
    (
        T entity,
        CancellationToken cancellationToken
    )
    {
        await _context
            .Set<T>()
            .AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddCollectionAsync
    (
        IEnumerable<T> entities,
        CancellationToken cancellationToken
    )
    {
        await _context
            .Set<T>()
            .AddRangeAsync(entities, cancellationToken);
    }

    public virtual async Task UpdateAsync
    (
        T entity,
        CancellationToken cancellationToken
    )
    {
        await Task.Run(() =>
        {
            _context
                .Set<T>()
                .Update(entity);
        }, cancellationToken);
    }

    public virtual async Task UpdateCollectionAsync
    (
        IEnumerable<T> entities,
        CancellationToken cancellationToken
    )
    {
        await Task.Run(() =>
        {
            _context
                .Set<T>()
                .UpdateRange(entities);
        }, cancellationToken);
    }

    public virtual async Task RemoveAsync
    (
        T entity,
        CancellationToken cancellationToken
    )
    {
        await Task.Run(() =>
        {
            _context
                .Set<T>()
                .Remove(entity);
        }, cancellationToken);
    }

    public virtual async Task RemoveCollectionAsync
    (
        IEnumerable<T> entities,
        CancellationToken cancellationToken
    )
    {
        await Task.Run(() =>
        {
            _context
                .Set<T>()
                .RemoveRange(entities);
        }, cancellationToken);
    }

    public virtual async Task<int> SaveChangesAsync
    (
        CancellationToken cancellationToken
    )
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<bool> CanConnectAsync
    (
        CancellationToken cancellationToken
    )
    {
        return await _context.CanConnectAsync
        (
            cancellationToken
        );
    }


    private static Func<IQueryable<T>, Expression<Func<T, object>>, IQueryable<T>>
        GetExpression()
    {
        return (current, include) => current.Include(include);
    }
}
