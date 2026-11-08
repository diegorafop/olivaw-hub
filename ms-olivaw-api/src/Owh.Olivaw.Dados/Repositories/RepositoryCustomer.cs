using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;

namespace Owh.Olivaw.Dados.Repositories;

public sealed class RepositoryCustomer(WriteDbContext context) : Repository<Customer>(context), IRepositoryCustomer
{
    public async Task<CustomerReadPage> ReadPageAsync(string search, string status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Set<Customer>().AsNoTracking().Where(item => item.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLowerInvariant();
            query = query.Where(item => item.Business.ToLower().Contains(term) || item.LegalName.ToLower().Contains(term) || (item.Document != null && item.Document.Contains(term)));
        }
        if (!string.IsNullOrEmpty(status)) query = query.Where(item => item.Status == status);
        return new CustomerReadPage
        {
            Total = await query.LongCountAsync(cancellationToken),
            Items = await query.OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.Id)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken)
        };
    }
    public async Task<IReadOnlyDictionary<string, long>> ReadCountsAsync(CancellationToken cancellationToken)
    {
        return await _context.Set<Customer>().AsNoTracking().Where(item => item.DeletedAt == null)
            .GroupBy(item => item.Status).Select(group => new { Status = group.Key, Count = group.LongCount() })
            .ToDictionaryAsync(item => item.Status, item => item.Count, cancellationToken);
    }
}
