using System;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Commands;
public sealed class UpdateInventoryResourceCommand
{
    public long Revision { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string AccountReference { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string ProviderResourceId { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Payer { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Scope { get; set; } = "platform";
    public string Status { get; set; } = "reported";
    public string Source { get; set; } = string.Empty;
    public string Monitoring { get; set; } = "none";
    public string ReferenceUrl { get; set; } = string.Empty;
    public decimal? CpuCapacity { get; set; }
    public decimal? MemoryCapacityGiB { get; set; }
    public decimal? StorageCapacityGiB { get; set; }
}
public sealed class UpdateInventoryResourceResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
