using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;

namespace Owh.Olivaw.Dados.Repositories;

public sealed class RepositoryManagedService(WriteDbContext context) : Repository<ManagedService>(context), IRepositoryManagedService
{
    public IQueryable<ManagedService> ReadRows(string search, string status, string type, Guid? customer)
    {
        var query = _context.Set<ManagedService>().AsNoTracking().Where(item => item.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLowerInvariant();
            query = query.Where(item => item.Name.ToLower().Contains(term) || item.TechnicalId.Contains(term));
        }
        if (!string.IsNullOrEmpty(status)) query = query.Where(item => item.Status == status);
        if (type == "unknown")
        {
            var classified = new[] { "dotnet", "angular", "postgres", "rabbitmq", "redis" };
            query = query.Where(item => !classified.Contains(item.Type));
        }
        else if (!string.IsNullOrEmpty(type)) query = query.Where(item => item.Type == type);
        if (customer.HasValue) query = query.Where(item => item.Customers.Any(link => link.Customer.ExternalId == customer.Value && link.EndedAt == null && link.DeletedAt == null && link.Customer.DeletedAt == null));
        return query;
    }
    public async Task<ManagedServiceReadPage> ReadPageAsync(string search, string status, string type, Guid? customer, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = ReadRows(search, status, type, customer);
        var total = await query.LongCountAsync(cancellationToken);
        var rows = await query.OrderBy(item => item.Name).ThenBy(item => item.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(item => new ManagedServiceReadItem
            {
                ExternalId = item.ExternalId, TechnicalId = item.TechnicalId, Name = item.Name, Type = item.Type,
                Status = item.Status, Scope = item.Scope, CreatedAt = item.CreatedAt,
                CustomerCount = item.Customers.Count(link => link.EndedAt == null && link.DeletedAt == null && link.Customer.DeletedAt == null)
            }).ToListAsync(cancellationToken);
        return new() { Total = total, Items = rows };
    }
}
