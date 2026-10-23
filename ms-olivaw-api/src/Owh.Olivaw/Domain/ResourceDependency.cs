using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;

public sealed class ResourceDependency : Entity
{
    public int SourceResourceId { get; private set; }
    public InventoryResource SourceResource { get; private set; } = null!;
    public int TargetResourceId { get; private set; }
    public InventoryResource TargetResource { get; private set; } = null!;
    public string Purpose { get; private set; } = string.Empty;
    public string Kind { get; private set; } = string.Empty;
    public long Revision { get; private set; } = 1;
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public Guid StartedByUserId { get; private set; }
    public Guid? EndedByUserId { get; private set; }
    public string StartedByName { get; private set; } = string.Empty;
    public string? EndedByName { get; private set; }
    private ResourceDependency() { }
    public static ResourceDependency Create(InventoryResource source, InventoryResource target, string purpose, string kind, Guid actor, string actorName, DateTime now)
    {
        if (source.Status == "retired") throw new ArgumentException("Recurso desativado.");
        if (purpose == null || purpose.Length > 300 || string.IsNullOrWhiteSpace(purpose)) throw new ArgumentException("Finalidade obrigatória, até 300 caracteres.");
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Operador inválido.");
        if (source.ExternalId == target.ExternalId) throw new ArgumentException("Um recurso não depende de si mesmo.");
        if (!InventoryRelations.IsCompatible(source.Type, target.Type, kind)) throw new ArgumentException("Relação incompatível com os tipos.");
        return new ResourceDependency
        {
            ExternalId = Guid.NewGuid(), SourceResource = source, SourceResourceId = source.Id, TargetResource = target, TargetResourceId = target.Id,
            Purpose = purpose, Kind = kind, StartedAt = now, CreatedAt = now, StartedByUserId = actor, StartedByName = actorName, ActorUserId = actor
        };
    }
    public void Close(Guid actor, string actorName, DateTime now)
    {
        if (EndedAt.HasValue) return;
        if (now < StartedAt) throw new ArgumentException("Data anterior ao início.");
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Operador inválido.");
        EndedAt = now; EndedByUserId = actor; EndedByName = actorName; ActorUserId = actor; UpdatedAt = now; Revision++;
    }
}
