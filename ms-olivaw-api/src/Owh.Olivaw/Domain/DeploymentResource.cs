using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;

public sealed class DeploymentResource : Entity
{
    public int InventoryResourceId { get; private set; }
    public InventoryResource InventoryResource { get; private set; } = null!;
    public int ServiceDeploymentId { get; private set; }
    public ServiceDeployment ServiceDeployment { get; private set; } = null!;
    public string Purpose { get; private set; } = string.Empty;
    public decimal? ReservedCpu { get; private set; }
    public decimal? ReservedMemoryGiB { get; private set; }
    public decimal? ReservedStorageGiB { get; private set; }
    public long Revision { get; private set; } = 1;
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public Guid StartedByUserId { get; private set; }
    public Guid? EndedByUserId { get; private set; }
    public string StartedByName { get; private set; } = string.Empty;
    public string? EndedByName { get; private set; }
    private DeploymentResource() { }
    public static DeploymentResource Create(InventoryResource source, ServiceDeployment target, string purpose, decimal? reservedCpu, decimal? reservedMemoryGiB, decimal? reservedStorageGiB, Guid actor, string actorName, DateTime now)
    {
        if (source.Status == "retired") throw new ArgumentException("Recurso desativado.");
        if (purpose == null || purpose.Length > 300 || string.IsNullOrWhiteSpace(purpose)) throw new ArgumentException("Finalidade obrigatória, até 300 caracteres.");
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Operador inválido.");
        if (target.EndedAt.HasValue) throw new ArgumentException("Implantação encerrada.");
        if (reservedCpu < 0 || reservedMemoryGiB < 0 || reservedStorageGiB < 0) throw new ArgumentException("Reserva inválida.");
        return new DeploymentResource
        {
            ExternalId = Guid.NewGuid(), InventoryResource = source, InventoryResourceId = source.Id, ServiceDeployment = target, ServiceDeploymentId = target.Id,
            Purpose = purpose, ReservedCpu = reservedCpu, ReservedMemoryGiB = reservedMemoryGiB, ReservedStorageGiB = reservedStorageGiB, StartedAt = now, CreatedAt = now, StartedByUserId = actor, StartedByName = actorName, ActorUserId = actor
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
