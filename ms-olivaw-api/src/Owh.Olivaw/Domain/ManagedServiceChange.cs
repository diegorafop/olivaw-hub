using System;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class ManagedServiceChange : Entity
{
    public int ManagedServiceId { get; private set; }
    public ManagedService ManagedService { get; private set; } = null!;
    public long Revision { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    public string PreviousStatus { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Scope { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    private ManagedServiceChange() { }
    internal static ManagedServiceChange Create(ManagedService service, string previous, Guid actor, string actorName, DateTime now) => new()
    {
        ExternalId = Guid.NewGuid(), ManagedService = service, ManagedServiceId = service.Id,
        Revision = service.Revision, ActorUserId = actor, ActorName = actorName, CreatedAt = now,
        PreviousStatus = previous, Status = service.Status, Name = service.Name, Type = service.Type,
        Scope = service.Scope, Description = service.Description, Owner = service.Owner
    };
}
