using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Queries;
public sealed class ReadServiceDeploymentByFilterQuery
{
    public Guid? ServiceExternalId { get; set; }
    public string? Environment { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
public sealed class ReadServiceDeploymentByFilterResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long Total { get; set; }
    public List<ServiceDeploymentItem> Items { get; set; } = new();
}
public sealed class ServiceDeploymentItem
{
    public Guid ExternalId { get; set; }
    public Guid ServiceExternalId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? EndedAt { get; set; }
}
