using System;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Commands;
public sealed class CreateDeploymentResourceCommand
{
    public Guid ResourceExternalId { get; set; }
    public Guid TargetExternalId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public decimal? ReservedCpu { get; set; }
    public decimal? ReservedMemoryGiB { get; set; }
    public decimal? ReservedStorageGiB { get; set; }
}
public sealed class CreateDeploymentResourceResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
