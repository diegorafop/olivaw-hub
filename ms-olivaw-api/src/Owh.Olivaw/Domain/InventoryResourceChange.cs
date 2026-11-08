using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;

public sealed class InventoryResourceChange : Entity
{
    public int InventoryResourceId { get; private set; }
    public InventoryResource InventoryResource { get; private set; } = null!;
    public long Revision { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    public DateTime? VerifiedAt { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Purpose { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string AccountReference { get; private set; } = string.Empty;
    public string Region { get; private set; } = string.Empty;
    public string ProviderResourceId { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    public string Payer { get; private set; } = string.Empty;
    public string Operator { get; private set; } = string.Empty;
    public string Scope { get; private set; } = "platform";
    public string Status { get; private set; } = "reported";
    public string Source { get; private set; } = string.Empty;
    public string Monitoring { get; private set; } = "none";
    public string ReferenceUrl { get; private set; } = string.Empty;
    public decimal? CpuCapacity { get; private set; }
    public decimal? MemoryCapacityGiB { get; private set; }
    public decimal? StorageCapacityGiB { get; private set; }
    public string Technology { get; private set; } = string.Empty;
    public string TechnologyVersion { get; private set; } = string.Empty;
    public string LogicalName { get; private set; } = string.Empty;
    public DateTime? RenewalAt { get; private set; }
    public DateTime? CertificateExpiresAt { get; private set; }
    public int? BackupIntervalHours { get; private set; }
    public int? BackupRetentionDays { get; private set; }
    private InventoryResourceChange() { }
    internal static InventoryResourceChange Create(InventoryResource resource, Guid actor, string actorName, DateTime now) => new()
    {
        ExternalId = Guid.NewGuid(), InventoryResource = resource, InventoryResourceId = resource.Id,
        Revision = resource.Revision, ActorUserId = actor, ActorName = actorName, CreatedAt = now, VerifiedAt = resource.VerifiedAt,
        Name = resource.Name,
        Type = resource.Type,
        Purpose = resource.Purpose,
        Provider = resource.Provider,
        AccountReference = resource.AccountReference,
        Region = resource.Region,
        ProviderResourceId = resource.ProviderResourceId,
        Owner = resource.Owner,
        Payer = resource.Payer,
        Operator = resource.Operator,
        Scope = resource.Scope,
        Status = resource.Status,
        Source = resource.Source,
        Monitoring = resource.Monitoring,
        ReferenceUrl = resource.ReferenceUrl,
        CpuCapacity = resource.CpuCapacity,
        MemoryCapacityGiB = resource.MemoryCapacityGiB,
        StorageCapacityGiB = resource.StorageCapacityGiB,
        Technology = resource.Technology,
        TechnologyVersion = resource.TechnologyVersion,
        LogicalName = resource.LogicalName,
        RenewalAt = resource.RenewalAt,
        CertificateExpiresAt = resource.CertificateExpiresAt,
        BackupIntervalHours = resource.BackupIntervalHours,
        BackupRetentionDays = resource.BackupRetentionDays,
    };
}
