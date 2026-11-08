using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Queries;
public sealed class ReadServiceDeploymentByIdQuery { public Guid ExternalId { get; set; } }
public sealed class ReadServiceDeploymentByIdResponse
{
    public Guid ExternalId { get; set; }
    public Guid ServiceExternalId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long Revision { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<ServiceDeploymentHistoryItem> History { get; set; } = new();
}
public sealed class ServiceDeploymentHistoryItem
{
    public long Revision { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
