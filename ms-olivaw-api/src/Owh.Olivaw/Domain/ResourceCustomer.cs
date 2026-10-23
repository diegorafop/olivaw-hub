using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;

public sealed class ResourceCustomer : Entity
{
    public int InventoryResourceId { get; private set; }
    public InventoryResource InventoryResource { get; private set; } = null!;
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public string Purpose { get; private set; } = string.Empty;
    public long Revision { get; private set; } = 1;
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public Guid StartedByUserId { get; private set; }
    public Guid? EndedByUserId { get; private set; }
    public string StartedByName { get; private set; } = string.Empty;
    public string? EndedByName { get; private set; }
    private ResourceCustomer() { }
    public static ResourceCustomer Create(InventoryResource source, Customer target, string purpose, Guid actor, string actorName, DateTime now)
    {
        if (source.Status == "retired") throw new ArgumentException("Recurso desativado.");
        if (purpose == null || purpose.Length > 300 || string.IsNullOrWhiteSpace(purpose)) throw new ArgumentException("Finalidade obrigatória, até 300 caracteres.");
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Operador inválido.");
        if (target.DeletedAt.HasValue) throw new ArgumentException("Cliente removido.");
        return new ResourceCustomer
        {
            ExternalId = Guid.NewGuid(), InventoryResource = source, InventoryResourceId = source.Id, Customer = target, CustomerId = target.Id,
            Purpose = purpose,  StartedAt = now, CreatedAt = now, StartedByUserId = actor, StartedByName = actorName, ActorUserId = actor
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
