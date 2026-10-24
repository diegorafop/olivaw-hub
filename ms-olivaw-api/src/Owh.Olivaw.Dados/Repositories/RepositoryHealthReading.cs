using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
namespace Owh.Olivaw.Dados.Repositories;
public sealed class RepositoryHealthReading(WriteDbContext context) : Repository<HealthReading>(context), IRepositoryHealthReading
{
    public IQueryable<HealthReading> ReadLatestRows(int[] resourceIds)
    {
        if (resourceIds == null || resourceIds.Length > 100) throw new ArgumentException("Limite de recursos excedido.");
        var readings = _context.Set<HealthReading>();
        return readings.AsNoTracking().Where(item => item.DeletedAt == null && resourceIds.Contains(item.InventoryResourceId))
            .Where(item => !readings.Any(next => next.DeletedAt == null && next.InventoryResourceId == item.InventoryResourceId && (next.ObservedAt > item.ObservedAt || next.ObservedAt == item.ObservedAt && next.Id > item.Id)));
    }
    public async Task<IReadOnlyList<HealthReading>> ReadLatestAsync(int[] resourceIds, CancellationToken token) => await ReadLatestRows(resourceIds).ToListAsync(token);
}
