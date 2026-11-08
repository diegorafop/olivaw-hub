using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Queries;
public sealed class ReadInventoryResourceByFilterQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; } = string.Empty;
    public string? Type { get; set; } = string.Empty;
    public string? Provider { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty;
    public string? Environment { get; set; } = string.Empty;
    public Guid? CustomerExternalId { get; set; }
    public bool? Shared { get; set; }
}
public sealed class ReadInventoryResourceByFilterResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long Total { get; set; }
    public List<InventoryResourceItem> Items { get; set; } = new();
}
public sealed class InventoryResourceItem
{
    public Guid ExternalId { get; set; }
    public string TechnicalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public int OriginCount { get; set; }
    public List<InventoryOriginItem> Origins { get; set; } = new();
}
public sealed class InventoryOriginItem
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
