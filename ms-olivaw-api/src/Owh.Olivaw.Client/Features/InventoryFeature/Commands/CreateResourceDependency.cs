using System;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Commands;
public sealed class CreateResourceDependencyCommand
{
    public Guid ResourceExternalId { get; set; }
    public Guid TargetExternalId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Kind { get; set; } = "depends-on";
}
public sealed class CreateResourceDependencyResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
