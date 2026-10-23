using System;
using System.Collections.Generic;

namespace Owh.Olivaw.Client.Features.ManagedServiceFeature.Commands;

public sealed class CreateManagedServiceCommand
{
    public string TechnicalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Scope { get; set; } = "customer";
    public string Status { get; set; } = "planned";
    public List<Guid> CustomerExternalIds { get; set; } = new();
}
public sealed class CreateManagedServiceResponse
{
    public Guid ExternalId { get; set; }
    public long Revision { get; set; }
}
