using System;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class CustomerChange : Entity
{
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public string ActorName { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string PreviousStatus { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    private CustomerChange() { }
    internal static CustomerChange Create(Guid actor, string actorName, string action, string previousStatus, string status, DateTime now)
    {
        return new CustomerChange { ExternalId = Guid.NewGuid(), ActorUserId = actor, ActorName = actorName, Action = action, PreviousStatus = previousStatus, Status = status, CreatedAt = now };
    }
}
