using System;
using System.Collections.Generic;

namespace Owh.Olivaw.Client.Features.ManagedServiceFeature.Queries;

public sealed class ReadManagedServiceByIdQuery { public Guid ExternalId { get; set; } }
public sealed class ReadManagedServiceByIdResponse
{
    public Guid ExternalId { get; set; }
    public long Revision { get; set; }
    public string TechnicalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public List<ManagedServiceCustomerItem> Customers { get; set; } = new();
    public List<ManagedServiceCustomerItem> PreviousCustomers { get; set; } = new();
    public List<ManagedServiceChangeItem> Changes { get; set; } = new();
}
public sealed class ManagedServiceCustomerItem
{
    public Guid CustomerExternalId { get; set; }
    public string Business { get; set; } = string.Empty;
    public string StartedByName { get; set; } = string.Empty;
    public string? EndedByName { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}
public sealed class ManagedServiceChangeItem
{
    public long Revision { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
}
