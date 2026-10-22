using System;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class CustomerContact : Entity
{
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Whatsapp { get; private set; } = string.Empty;
    private CustomerContact() { }
    public static CustomerContact Create(string name, string role, string email, string whatsapp, Guid actor, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Informe o nome do contato.");
        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(whatsapp)) throw new ArgumentException("Informe um contato.");
        return new CustomerContact { ExternalId = Guid.NewGuid(), Name = name, Role = role, Email = email, Whatsapp = whatsapp, ActorUserId = actor, CreatedAt = now };
    }
}
