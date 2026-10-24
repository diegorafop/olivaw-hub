using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Queries;
public sealed class ReadInventoryResourceByIdQuery { public Guid ExternalId { get; set; } }
public sealed class ReadInventoryResourceByIdResponse
{
    public Guid ExternalId { get; set; }
    public string TechnicalId { get; set; } = string.Empty;
    public bool ProviderIdentityLocked { get; set; }
    public long Revision { get; set; }
    public DateTime? VerifiedAt { get; set; }
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
    public string Technology { get; set; } = string.Empty;
    public string TechnologyVersion { get; set; } = string.Empty;
    public string LogicalName { get; set; } = string.Empty;
    public DateTime? RenewalAt { get; set; }
    public DateTime? CertificateExpiresAt { get; set; }
    public int? BackupIntervalHours { get; set; }
    public int? BackupRetentionDays { get; set; }
    public int CustomerCount { get; set; }
    public List<InventoryCustomerItem> Customers { get; set; } = new();
    public decimal? ReservedCpu { get; set; }
    public decimal? ReservedMemoryGiB { get; set; }
    public decimal? ReservedStorageGiB { get; set; }
    public int IncompleteReservations { get; set; }
    public int OriginCount { get; set; }
    public List<InventoryOriginItem> Origins { get; set; } = new();
    public List<InventoryResourceHistoryItem> History { get; set; } = new();
}
public sealed class InventoryCustomerItem { public Guid ExternalId { get; set; } public string Business { get; set; } = string.Empty; }
public sealed class InventoryResourceHistoryItem
{
    public long Revision { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
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
    public string Technology { get; set; } = string.Empty;
    public string TechnologyVersion { get; set; } = string.Empty;
    public string LogicalName { get; set; } = string.Empty;
    public DateTime? RenewalAt { get; set; }
    public DateTime? CertificateExpiresAt { get; set; }
    public int? BackupIntervalHours { get; set; }
    public int? BackupRetentionDays { get; set; }
}
