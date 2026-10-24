using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Olivaw.Identity.Repositories;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Tests.Mocks;
internal sealed class PortalIdentityStoreFake : IIdentityStore
{
    private readonly List<Entity> _items = new();
    internal PortalIdentityStoreFake(params Entity[] items) { _items.AddRange(items); }
    public Task<T?> FindAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken token) where T : Entity { token.ThrowIfCancellationRequested(); return Task.FromResult(_items.OfType<T>().Where(item => item.DeletedAt == null).FirstOrDefault(predicate.Compile())); }
    public Task<T?> FindIncludingRemovedAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken token) where T : Entity => Task.FromResult(_items.OfType<T>().FirstOrDefault(predicate.Compile()));
    public Task<IReadOnlyList<T>> ListAsync<T>(Expression<Func<T, bool>> predicate, int page, int size, CancellationToken token) where T : Entity => Task.FromResult<IReadOnlyList<T>>(_items.OfType<T>().Where(item => item.DeletedAt == null).Where(predicate.Compile()).Skip((page - 1) * size).Take(size).ToArray());
    public Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken token) where T : Entity => Task.FromResult(_items.OfType<T>().Any(predicate.Compile()));
    public void Add<T>(T entity) where T : Entity { if (entity.Id == 0) entity.Id = _items.Count + 1; _items.Add(entity); }
    public Task SaveAsync(CancellationToken token) { token.ThrowIfCancellationRequested(); return Task.CompletedTask; }
    public Task<IReadOnlyList<string>> PermissionsAsync(int id, CancellationToken token) => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
}
