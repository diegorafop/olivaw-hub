using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class InventoryResource : Entity
{
    public string TechnicalId { get; private set; } = string.Empty;
    public string? ProviderIdentityKey { get; private set; }
    public long Revision { get; private set; }
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
    public string Scope { get; private set; } = "shared";
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
    public ICollection<InventoryResourceChange> Changes { get; private set; } = new List<InventoryResourceChange>();
    private InventoryResource() { }
    public static string? IdentityKey(InventoryResourceData data)
    {
        if (string.IsNullOrEmpty(data.ProviderResourceId)) return null;
        var parts = new[] { data.Provider.ToLowerInvariant(), data.AccountReference.ToLowerInvariant(), data.Region.ToLowerInvariant(), data.Type, data.ProviderResourceId };
        var canonical = new StringBuilder();
        foreach (var part in parts) canonical.Append(part.Length).Append(':').Append(part);
        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString()))).Replace("-", "");
    }
    public static InventoryResource Create(string technicalId, InventoryResourceData data, Guid actor, string actorName, DateTime now)
    {
        if (technicalId == null) throw new ArgumentNullException(nameof(technicalId));
        if (technicalId.Length > 80) throw new ArgumentException("Identificador inválido.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(technicalId, "^[a-z0-9]+(?:-[a-z0-9]+)*$")) throw new ArgumentException("Identificador inválido.");
        var resource = new InventoryResource { TechnicalId = technicalId, ExternalId = Guid.NewGuid(), CreatedAt = now };
        resource.Changes.Add(resource.Apply(data, actor, actorName, now));
        return resource;
    }
    public InventoryResourceChange Apply(InventoryResourceData data, Guid actor, string actorName, DateTime now)
    {
        data.Validate();
        if (actor == Guid.Empty) throw new ArgumentException("Operador obrigatório.");
        if (string.IsNullOrWhiteSpace(actorName)) throw new ArgumentException("Operador obrigatório.");
        if (actorName.Length > 120) throw new ArgumentException("Operador inválido.");
        var key = IdentityKey(data);
        if (ProviderIdentityKey != null && ProviderIdentityKey != key) throw new ArgumentException("A identidade externa é imutável; cadastre um novo recurso.");
        if (data.Status != "verified") VerifiedAt = null;
        else if (Status != "verified") VerifiedAt = now;
        Name = data.Name;
        Type = data.Type;
        Purpose = data.Purpose;
        Provider = data.Provider;
        AccountReference = data.AccountReference;
        Region = data.Region;
        ProviderResourceId = data.ProviderResourceId;
        Owner = data.Owner;
        Payer = data.Payer;
        Operator = data.Operator;
        Scope = data.Scope;
        Status = data.Status;
        Source = data.Source;
        Monitoring = data.Monitoring;
        ReferenceUrl = data.ReferenceUrl;
        CpuCapacity = data.CpuCapacity;
        MemoryCapacityGiB = data.MemoryCapacityGiB;
        StorageCapacityGiB = data.StorageCapacityGiB;
        Technology = data.Technology;
        TechnologyVersion = data.TechnologyVersion;
        LogicalName = data.LogicalName;
        RenewalAt = data.RenewalAt;
        CertificateExpiresAt = data.CertificateExpiresAt;
        BackupIntervalHours = data.BackupIntervalHours;
        BackupRetentionDays = data.BackupRetentionDays;
        ProviderIdentityKey = key; ActorUserId = actor; Revision++;
        if (Revision > 1) UpdatedAt = now;
        return InventoryResourceChange.Create(this, actor, actorName, now);
    }
}
