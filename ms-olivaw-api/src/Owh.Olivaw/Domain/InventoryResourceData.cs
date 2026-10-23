using System;

namespace Owh.Olivaw.Domain;

public sealed class InventoryResourceData
{
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
    public static bool IsType(string value) => value is "compute" or "volume" or "database-instance" or "database" or "message-broker" or "message-namespace" or "cache-instance" or "cache-namespace" or "domain" or "subdomain" or "public-ip" or "dns-zone" or "dns-record" or "proxy" or "certificate" or "backup" or "observability" or "registry" or "external-service" or "other";
    public static bool IsStatus(string value) => value is "reported" or "verified" or "retired";
    public static bool IsMonitoring(string value) => value is "none" or "planned";
    public static bool IsScope(string value) => value is "platform" or "customer";
    public static bool IsReferenceUrl(string value)
    {
        if (value == "") return true;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme is not ("https" or "http")) return false;
        if (uri.UserInfo.Length > 0) return false;
        if (uri.Query.Length > 0) return false;
        return uri.Fragment.Length == 0;
    }
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Nome obrigatório.");
        if (!IsType(Type)) throw new ArgumentException("Tipo inválido.");
        if (!IsStatus(Status)) throw new ArgumentException("Situação inválida.");
        if (!IsScope(Scope)) throw new ArgumentException("Escopo inválido.");
        if (!IsMonitoring(Monitoring)) throw new ArgumentException("Monitoramento inválido.");
        if (!IsReferenceUrl(ReferenceUrl)) throw new ArgumentException("Referência HTTP(S) inválida.");
        if (Name == null) throw new ArgumentNullException(nameof(Name));
        if (Name.Length > 120) throw new ArgumentException("Campo muito longo.", nameof(Name));
        if (Type == null) throw new ArgumentNullException(nameof(Type));
        if (Type.Length > 30) throw new ArgumentException("Campo muito longo.", nameof(Type));
        if (Purpose == null) throw new ArgumentNullException(nameof(Purpose));
        if (Purpose.Length > 1000) throw new ArgumentException("Campo muito longo.", nameof(Purpose));
        if (Provider == null) throw new ArgumentNullException(nameof(Provider));
        if (Provider.Length > 80) throw new ArgumentException("Campo muito longo.", nameof(Provider));
        if (AccountReference == null) throw new ArgumentNullException(nameof(AccountReference));
        if (AccountReference.Length > 120) throw new ArgumentException("Campo muito longo.", nameof(AccountReference));
        if (Region == null) throw new ArgumentNullException(nameof(Region));
        if (Region.Length > 80) throw new ArgumentException("Campo muito longo.", nameof(Region));
        if (ProviderResourceId == null) throw new ArgumentNullException(nameof(ProviderResourceId));
        if (ProviderResourceId.Length > 180) throw new ArgumentException("Campo muito longo.", nameof(ProviderResourceId));
        if (Owner == null) throw new ArgumentNullException(nameof(Owner));
        if (Owner.Length > 120) throw new ArgumentException("Campo muito longo.", nameof(Owner));
        if (Payer == null) throw new ArgumentNullException(nameof(Payer));
        if (Payer.Length > 120) throw new ArgumentException("Campo muito longo.", nameof(Payer));
        if (Operator == null) throw new ArgumentNullException(nameof(Operator));
        if (Operator.Length > 120) throw new ArgumentException("Campo muito longo.", nameof(Operator));
        if (Scope == null) throw new ArgumentNullException(nameof(Scope));
        if (Scope.Length > 20) throw new ArgumentException("Campo muito longo.", nameof(Scope));
        if (Status == null) throw new ArgumentNullException(nameof(Status));
        if (Status.Length > 20) throw new ArgumentException("Campo muito longo.", nameof(Status));
        if (Source == null) throw new ArgumentNullException(nameof(Source));
        if (Source.Length > 500) throw new ArgumentException("Campo muito longo.", nameof(Source));
        if (Monitoring == null) throw new ArgumentNullException(nameof(Monitoring));
        if (Monitoring.Length > 20) throw new ArgumentException("Campo muito longo.", nameof(Monitoring));
        if (ReferenceUrl == null) throw new ArgumentNullException(nameof(ReferenceUrl));
        if (ReferenceUrl.Length > 300) throw new ArgumentException("Campo muito longo.", nameof(ReferenceUrl));
        if (CpuCapacity < 0) throw new ArgumentException("Capacidade inválida.");
        if (MemoryCapacityGiB < 0) throw new ArgumentException("Capacidade inválida.");
        if (StorageCapacityGiB < 0) throw new ArgumentException("Capacidade inválida.");
        if (Status == "verified" && string.IsNullOrWhiteSpace(Source)) throw new ArgumentException("Informe a fonte da conferência.");
        if (ProviderResourceId.Length > 0 && string.IsNullOrWhiteSpace(Provider)) throw new ArgumentException("Informe o provedor para a identidade externa.");
    }
}
