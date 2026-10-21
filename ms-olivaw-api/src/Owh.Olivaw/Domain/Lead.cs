using System;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class Lead : Entity
{
    public string WriteKeyHash { get; private set; } = string.Empty;
    public long Revision { get; private set; }
    public string Snapshot { get; private set; } = string.Empty;
    public string Stage { get; private set; } = string.Empty;

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
