using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Repositories;

// Paginação, contagem e projeção de vínculos no banco não estão disponíveis no contrato genérico.
public interface IRepositoryManagedService : IRepository<ManagedService>
{
    Task<ManagedServiceReadPage> ReadPageAsync(string search, string status, string type, Guid? customer, int page, int pageSize, CancellationToken cancellationToken);
}
public sealed class ManagedServiceReadPage
{
    public long Total { get; set; }
    public List<ManagedServiceReadItem> Items { get; set; } = new();
}
public sealed class ManagedServiceReadItem
{
    public Guid ExternalId { get; set; }
    public string TechnicalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
}
