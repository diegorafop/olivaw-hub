using System;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class Lead : Entity
{
    public string WriteKeyHash { get; private set; } = string.Empty;
    public long Revision { get; private set; }
    public string Snapshot { get; private set; } = string.Empty;
    public string Stage { get; private set; } = string.Empty;

    public string FollowUpStatus { get; private set; } = "new";
    public long FollowUpRevision { get; private set; }
    public DateTime? FollowUpUpdatedAt { get; private set; }

    public static bool IsFollowUpStatus(string status) => status is "new" or "conversation" or "analysis" or "development" or "validation" or "delivered" or "discontinued";

    public LeadChange UpdateFollowUp(string status, string note, Guid actor, string actorName, DateTime now)
    {
        if (!IsFollowUpStatus(status)) throw new ArgumentException("Etapa inválida.", nameof(status));
        if (note == null || note.Length > 3000) throw new ArgumentException("Nota inválida.", nameof(note));
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Operador inválido.", nameof(actor));
        if (status == FollowUpStatus && string.IsNullOrWhiteSpace(note)) throw new ArgumentException("Informe uma nota ou altere a etapa.", nameof(note));
        var previous = FollowUpStatus;
        FollowUpStatus = status;
        FollowUpRevision++;
        FollowUpUpdatedAt = now;
        return LeadChange.Create(this, previous, note, actor, actorName, now);
    }

    private Lead() { }

    public static Lead Create(Guid externalId, string writeKeyHash, long revision, string snapshot, DateTime now)
    {
        if (externalId == Guid.Empty || writeKeyHash.Length != 64)
        {
            throw new ArgumentException("Identificação inválida para o lead.");
        }
        var lead = new Lead { ExternalId = externalId, WriteKeyHash = writeKeyHash, CreatedAt = now };
        lead.Apply(revision, snapshot, now);
        lead.UpdatedAt = null;
        return lead;
    }

    public void Apply(long revision, string snapshot, DateTime now)
    {
        if (revision <= Revision || string.IsNullOrWhiteSpace(snapshot) || snapshot.Length > 32000)
        {
            throw new ArgumentException("Revisão inválida para o lead.");
        }
        Revision = revision;
        Snapshot = snapshot;
        Stage = "submitted";
        UpdatedAt = now;
    }
}
