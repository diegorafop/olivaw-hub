using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Repositories;

// Grafo, beneficiários deduplicados, projeções, contagem e paginação não existem no contrato genérico.
public interface IRepositoryInventoryResource : IRepository<InventoryResource>
{
    Task<InventoryResourcePage> ReadPageAsync(InventoryResourceFilter filter, CancellationToken token);
    Task<InventoryResourceContext> ReadContextAsync(int resourceId, CancellationToken token);
    Task<InventoryLinkPage> ReadLinksAsync(InventoryLinkFilter filter, CancellationToken token);
    Task<InventoryGraphData> ReadGraphAsync(CancellationToken token);
}
public sealed class InventoryResourceFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Search { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public Guid? CustomerExternalId { get; set; }
    public bool? Shared { get; set; }
}
public sealed class InventoryResourcePage
{
    public long Total { get; set; }
    public bool LimitExceeded { get; set; }
    public List<InventoryResourceReadItem> Items { get; set; } = new();
}
public sealed class InventoryResourceReadItem
{
    public InventoryResource Resource { get; set; } = null!;
    public InventoryResourceContext Context { get; set; } = new();
}
public sealed class InventoryResourceContext
{
    public bool LimitExceeded { get; set; }
    public int CustomerCount { get; set; }
    public List<InventoryBeneficiary> Customers { get; set; } = new();
    public int OriginCount { get; set; }
    public List<InventoryReadOrigin> Origins { get; set; } = new();
    public decimal? ReservedCpu { get; set; }
    public decimal? ReservedMemoryGiB { get; set; }
    public decimal? ReservedStorageGiB { get; set; }
    public int IncompleteReservations { get; set; }
}
public sealed class InventoryReadOrigin
{
    public string Kind { get; set; } = string.Empty;
    public Guid? FromResourceExternalId { get; set; }
    public string FromResourceName { get; set; } = string.Empty;
    public Guid? DeploymentExternalId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string RelationshipKind { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}
public sealed class InventoryBeneficiary
{
    public int Id { get; set; }
    public Guid ExternalId { get; set; }
    public string Business { get; set; } = string.Empty;
}
public sealed class InventoryNode
{
    public int Id { get; set; }
    public Guid ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
}
public sealed class InventoryDeclaredUse
{
    public int ResourceId { get; set; }
    public int ServiceId { get; set; }
    public Guid DeploymentExternalId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public decimal? ReservedCpu { get; set; }
    public decimal? ReservedMemoryGiB { get; set; }
    public decimal? ReservedStorageGiB { get; set; }
}
public sealed class InventoryGraphData
{
    public bool LimitExceeded { get; set; }
    public List<InventoryNode> Nodes { get; set; } = new();
    public List<InventoryEdge> Edges { get; set; } = new();
    public List<InventoryOrigin> Origins { get; set; } = new();
    public List<InventoryBeneficiary> Customers { get; set; } = new();
    public List<InventoryDeclaredUse> Uses { get; set; } = new();
}

public sealed class InventoryLinkFilter
{
    public Guid? ResourceExternalId { get; set; }
    public Guid? DeploymentExternalId { get; set; }
    public string Kind { get; set; } = "customer";
    public bool IncludeEnded { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
public sealed class InventoryLinkPage
{
    public long Total { get; set; }
    public List<InventoryLinkReadItem> Items { get; set; } = new();
}
public sealed class InventoryLinkReadItem
{
    public Guid ExternalId { get; set; }
    public Guid ResourceExternalId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public Guid TargetExternalId { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public string RelationshipKind { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public long Revision { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string StartedByName { get; set; } = string.Empty;
    public string? EndedByName { get; set; }
    public decimal? ReservedCpu { get; set; }
    public decimal? ReservedMemoryGiB { get; set; }
    public decimal? ReservedStorageGiB { get; set; }
}
