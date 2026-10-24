using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
namespace Owh.Olivaw.Dados.Repositories;
public sealed class RepositoryDeliveryNote(WriteDbContext context) : Repository<DeliveryNote>(context), IRepositoryDeliveryNote
{
    public IQueryable<DeliveryNote> ReadPublishedRows(DeliveryTimelineFilter filter)
    {
        if (filter.CustomerId < 1) throw new ArgumentException("Empresa obrigatória.");
        var rows = _context.Set<DeliveryNote>().AsNoTracking().Where(item => item.CustomerId == filter.CustomerId && item.DeletedAt == null && item.Status == "published" && item.PublishedAt != null && item.DeliveredAt != null && item.DeliveredAt <= filter.Now);
        if (filter.ServiceExternalId.HasValue) rows = rows.Where(item => item.ManagedService != null && item.ManagedService.ExternalId == filter.ServiceExternalId.Value);
        if (filter.Category.Length > 0) rows = rows.Where(item => item.Category == filter.Category);
        if (filter.FromAt.HasValue) rows = rows.Where(item => item.DeliveredAt >= filter.FromAt.Value);
        if (filter.ToAt.HasValue) rows = rows.Where(item => item.DeliveredAt <= filter.ToAt.Value);
        if (filter.BeforeAt.HasValue) rows = rows.Where(item => item.DeliveredAt < filter.BeforeAt.Value || item.DeliveredAt == filter.BeforeAt.Value && item.Id < filter.BeforeId);
        return rows.OrderByDescending(item => item.DeliveredAt).ThenByDescending(item => item.Id).Take(21).Include(item => item.ManagedService).Include(item => item.SupportRequest);
    }
    public async Task<IReadOnlyList<DeliveryNote>> ReadPublishedAsync(DeliveryTimelineFilter filter, CancellationToken token) => await ReadPublishedRows(filter).ToListAsync(token);
}
