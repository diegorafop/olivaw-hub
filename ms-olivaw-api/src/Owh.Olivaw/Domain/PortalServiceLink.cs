using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class PortalServiceLink : Entity
{
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public int ManagedServiceId { get; private set; }
    public ManagedService ManagedService { get; private set; } = null!;
    public int? HealthResourceId { get; private set; }
    public InventoryResource? HealthResource { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string PublicUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public long Revision { get; private set; }
    private PortalServiceLink() { }
    public static PortalServiceLink Create(Customer customer, ManagedService service, InventoryResource? resource, string label, string url, Guid actor, DateTime now)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));
        if (service == null) throw new ArgumentNullException(nameof(service));
        if (actor == Guid.Empty) throw new ArgumentException("Autor obrigatório.");
        if (string.IsNullOrWhiteSpace(label) || label.Length > 120) throw new ArgumentException("Nome público inválido.");
        if (!InventoryResourceData.IsReferenceUrl(url)) throw new ArgumentException("Endereço inválido.");
        if (url.Length > 300) throw new ArgumentException("Endereço muito longo.");
        if (url.Length > 0 && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Use HTTPS para o endereço público.");
        return new() { ExternalId = Guid.NewGuid(), Customer = customer, CustomerId = customer.Id, ManagedService = service, ManagedServiceId = service.Id, HealthResource = resource, HealthResourceId = resource?.Id, Label = label, PublicUrl = url, Revision = 1, ActorUserId = actor, CreatedAt = now };
    }
    public void Update(InventoryResource? resource, string label, string url, bool active, Guid actor, DateTime now)
    {
        if (actor == Guid.Empty) throw new ArgumentException("Autor obrigatório.");
        if (string.IsNullOrWhiteSpace(label) || label.Length > 120) throw new ArgumentException("Nome público inválido.");
        if (!InventoryResourceData.IsReferenceUrl(url) || url.Length > 300) throw new ArgumentException("Endereço inválido.");
        if (url.Length > 0 && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Use HTTPS para o endereço público.");
        HealthResource = resource; HealthResourceId = resource?.Id; Label = label; PublicUrl = url; IsActive = active; ActorUserId = actor; UpdatedAt = now; Revision++;
    }
    public void Revoke(Guid actor, DateTime now) { if (actor == Guid.Empty) throw new ArgumentException("Autor obrigatório."); IsActive = false; Revision++; ActorUserId = actor; UpdatedAt = now; }
}
