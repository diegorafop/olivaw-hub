using System;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class LeadChange : Entity
{
    public int LeadId { get; private set; }
    public Lead Lead { get; private set; } = null!;
    public long Revision { get; private set; }
    public string PreviousStatus { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string Note { get; private set; } = string.Empty;
    public string ActorName { get; private set; } = string.Empty;

    private LeadChange() { }

    internal static LeadChange Create(Lead lead, string previous, string note, Guid actor, string actorName, DateTime now) => new()
    {
        ExternalId = Guid.NewGuid(), Lead = lead, LeadId = lead.Id, Revision = lead.FollowUpRevision,
        PreviousStatus = previous, Status = lead.FollowUpStatus, Note = note, ActorUserId = actor,
        ActorName = actorName, CreatedAt = now
    };
}
