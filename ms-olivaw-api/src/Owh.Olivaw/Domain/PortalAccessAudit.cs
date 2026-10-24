using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class PortalAccessAudit : Entity
{
    public Guid PortalUserExternalId { get; private set; }
    public int? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public string ActorAuthority { get; private set; } = string.Empty;
    public string ActorName { get; private set; } = string.Empty;
    private PortalAccessAudit() { }
    public static PortalAccessAudit Create(Guid user, Customer? customer, string action, string details, string authority, Guid actor, string name, DateTime now)
    {
        if (user == Guid.Empty || actor == Guid.Empty) throw new ArgumentException("Autor obrigatório.");
        if (action is not ("invited" or "accepted" or "revoked" or "reset")) throw new ArgumentException("Ação inválida.");
        if (authority is not ("administration" or "portal")) throw new ArgumentException("Autoridade inválida.");
        if (details == null || details.Length > 1000) throw new ArgumentException("Detalhes inválidos.");
        if (string.IsNullOrWhiteSpace(name) || name.Length > 120) throw new ArgumentException("Autor inválido.");
        return new() { ExternalId = Guid.NewGuid(), PortalUserExternalId = user, Customer = customer, CustomerId = customer?.Id, Action = action, Details = details, ActorAuthority = authority, ActorUserId = actor, ActorName = name, CreatedAt = now };
    }
}
