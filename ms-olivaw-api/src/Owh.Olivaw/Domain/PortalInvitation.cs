using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class PortalInvitation : Entity
{
    public Guid PortalUserExternalId { get; private set; }
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public string Email { get; private set; } = string.Empty;
    public string Role { get; private set; } = "viewer";
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid SecurityStampAtIssue { get; private set; }
    public long Revision { get; private set; }
    private PortalInvitation() { }
    public static PortalInvitation Create(Guid user, Customer customer, string email, string role, string tokenHash, Guid actor, DateTime now, Guid securityStamp)
    {
        if (user == Guid.Empty || actor == Guid.Empty) throw new ArgumentException("Usuário obrigatório.");
        if (customer == null) throw new ArgumentNullException(nameof(customer));
        if (string.IsNullOrWhiteSpace(email) || email.Length > 254) throw new ArgumentException("E-mail inválido.");
        if (!PortalMembership.IsRole(role)) throw new ArgumentException("Papel inválido.");
        if (tokenHash == null || tokenHash.Length != 64) throw new ArgumentException("Convite inválido.");
        return new() { ExternalId = Guid.NewGuid(), PortalUserExternalId = user, Customer = customer, CustomerId = customer.Id, Email = email, Role = role, TokenHash = tokenHash, SecurityStampAtIssue = securityStamp, ExpiresAt = now.AddHours(48), InvitedByUserId = actor, ActorUserId = actor, Revision = 1, CreatedAt = now };
    }
    public bool CanAccept(Guid user, DateTime now) => user == PortalUserExternalId && AcceptedAt == null && RevokedAt == null && ExpiresAt > now;
    public void Revoke(Guid actor, DateTime now)
    {
        if (actor == Guid.Empty) throw new ArgumentException("Autor obrigatório.");
        RevokedAt = now; ActorUserId = actor; UpdatedAt = now; Revision++;
    }
    public void Accept(Guid user, DateTime now)
    {
        if (!CanAccept(user, now)) throw new ArgumentException("Convite indisponível.");
        AcceptedAt = now; AcceptedByUserId = user; ActorUserId = user; UpdatedAt = now; Revision++;
    }
}
