using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Repositories;

public interface IRepositoryCustomer : IRepository<Customer>
{
    Task<CustomerReadPage> ReadPageAsync(string search, string status, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<string, long>> ReadCountsAsync(CancellationToken cancellationToken);
}
public sealed class CustomerReadPage
{
    public long Total { get; set; }
    public List<Customer> Items { get; set; } = new();
}
