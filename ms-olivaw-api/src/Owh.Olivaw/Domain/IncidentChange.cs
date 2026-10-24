using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class IncidentChange : Entity
{
    public int IncidentId { get; private set; }
    public Incident Incident { get; private set; } = null!;
    public long Revision { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Impact { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    public string NextAction { get; private set; } = string.Empty;
    public string InternalNote { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    private IncidentChange() { }
    internal static IncidentChange Create(Incident entity, string actorName, DateTime now) => new()
    {
        ExternalId = Guid.NewGuid(), Incident = entity, IncidentId = entity.Id, Revision = entity.Revision, ActorUserId = entity.ActorUserId, ActorName = actorName, CreatedAt = now,
        Title = entity.Title,
        Impact = entity.Impact,
        Owner = entity.Owner,
        NextAction = entity.NextAction,
        InternalNote = entity.InternalNote,
        Severity = entity.Severity,
        Status = entity.Status,
    };
}
