using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;

public sealed class InventoryRevision : Entity
{
    public string Key { get; private set; } = "topology";
    public long Revision { get; private set; }
    private InventoryRevision() { }
    public static InventoryRevision Create(Guid actor, DateTime now)
    {
        var revision = new InventoryRevision { ExternalId = Guid.NewGuid(), CreatedAt = now };
        revision.Touch(actor, now);
        return revision;
    }
    public void Touch(Guid actor, DateTime now) { ActorUserId = actor; UpdatedAt = now; Revision++; }
}
