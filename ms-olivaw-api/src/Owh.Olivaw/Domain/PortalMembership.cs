using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class PortalMembership : Entity
{
    public Guid PortalUserExternalId { get; private set; }
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public string Role { get; private set; } = "viewer";
    public bool IsActive { get; private set; }
    public long Revision { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    private PortalMembership() { }
    public static bool IsRole(string role) => role is "viewer" or "requester" or "manager";
    public bool CanRequest => IsActive && Role is "requester" or "manager";
    public bool CanReadBilling => IsActive && Role == "manager";
    public static PortalMembership Create(Guid user, Customer customer, string role, Guid actor, string actorName, DateTime now)
    {
        if (user == Guid.Empty) throw new ArgumentException("Usuário obrigatório.");
        if (customer == null) throw new ArgumentNullException(nameof(customer));
        var item = new PortalMembership { ExternalId = Guid.NewGuid(), PortalUserExternalId = user, Customer = customer, CustomerId = customer.Id, CreatedAt = now };
        item.Grant(role, actor, actorName, now); return item;
    }
    public void Grant(string role, Guid actor, string actorName, DateTime now)
    {
        if (!IsRole(role)) throw new ArgumentException("Papel inválido.");
        ValidateActor(actor, actorName);
        Role = role; IsActive = true; ActorUserId = actor; ActorName = actorName; Revision++; UpdatedAt = now;
    }
    public void Revoke(Guid actor, string actorName, DateTime now)
    {
        ValidateActor(actor, actorName); IsActive = false; ActorUserId = actor; ActorName = actorName; Revision++; UpdatedAt = now;
    }
    private static void ValidateActor(Guid actor, string name)
    {
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(name) || name.Length > 120) throw new ArgumentException("Autor inválido.");
    }
}
