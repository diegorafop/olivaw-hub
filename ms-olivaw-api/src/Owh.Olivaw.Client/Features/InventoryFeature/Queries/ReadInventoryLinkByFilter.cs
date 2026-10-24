using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Queries;
public sealed class ReadInventoryLinkByFilterQuery
{
    public Guid? ResourceExternalId { get; set; }
    public Guid? DeploymentExternalId { get; set; }
    public string Kind { get; set; } = "customer";
    public bool IncludeEnded { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
public sealed class ReadInventoryLinkByFilterResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long Total { get; set; }
    public List<InventoryLinkItem> Items { get; set; } = new();
}
public sealed class InventoryLinkItem
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
