using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
namespace Owh.Olivaw.Dados.Repositories;
public sealed class RepositoryServiceDeployment(WriteDbContext context) : Repository<ServiceDeployment>(context), IRepositoryServiceDeployment
{
    public async Task<ServiceDeploymentPage> ReadPageAsync(Guid? serviceId, string environment, int page, int size, CancellationToken token)
    {
        var query = _context.Set<ServiceDeployment>().AsNoTracking().Include(item => item.ManagedService).Where(item => item.DeletedAt == null && item.ManagedService.DeletedAt == null);
        if (serviceId.HasValue) query = query.Where(item => item.ManagedService.ExternalId == serviceId.Value);
        if (environment.Length > 0) query = query.Where(item => item.Environment == environment);
        return new() { Total = await query.LongCountAsync(token), Items = await query.OrderByDescending(item => item.Id).Skip((page - 1) * size).Take(size).ToListAsync(token) };
    }
}
