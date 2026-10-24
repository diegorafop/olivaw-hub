using System;
using System.Collections.Generic;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class Incident : Entity
{
    public int ResourceId { get; private set; }
    public InventoryResource Resource { get; private set; } = null!;
    public long Revision { get; private set; }
    public string? OpenKey { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Impact { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    public string NextAction { get; private set; } = string.Empty;
    public string InternalNote { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public string Status { get; private set; } = "detected";
    public ICollection<IncidentChange> Changes { get; private set; } = new List<IncidentChange>();
    private Incident() { }
    public static Incident Create(InventoryResource anchor, IncidentData data, Guid actor, string actorName, DateTime now)
    {
        if (anchor == null) throw new ArgumentNullException(nameof(anchor));
        if (data.Status != "detected") throw new ArgumentException("Estado inicial inválido.");
        var entity = new Incident { ExternalId = Guid.NewGuid(), Resource = anchor, ResourceId = anchor.Id, CreatedAt = now };
        entity.Changes.Add(entity.Apply(data, actor, actorName, now));
        return entity;
    }
    public IncidentChange Apply(IncidentData data, Guid actor, string actorName, DateTime now)
    {
        data.Validate();
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Autor obrigatório.");
        if (data.Status == "resolved" && Status != "observation") throw new ArgumentException("Coloque em observação e confirme a recuperação antes de resolver.");
        Title = data.Title;
        Impact = data.Impact;
        Owner = data.Owner;
        NextAction = data.NextAction;
        InternalNote = data.InternalNote;
        Severity = data.Severity; Status = data.Status;
        ActorUserId = actor; Revision++;
        if (Revision > 1) UpdatedAt = now;
        OpenKey = null;
        if (Status != "resolved") OpenKey = Resource.ExternalId.ToString("N");
        return IncidentChange.Create(this, actorName, now);
    }
}
