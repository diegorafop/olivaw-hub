using System;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class ManagedServiceCustomer : Entity
{
    public int ManagedServiceId { get; private set; }
    public ManagedService ManagedService { get; private set; } = null!;
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public Guid StartedByUserId { get; private set; }
    public Guid? EndedByUserId { get; private set; }
    public string StartedByName { get; private set; } = string.Empty;
    public string? EndedByName { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    private ManagedServiceCustomer() { }

    public static ManagedServiceCustomer Create(ManagedService service, Customer customer, Guid actor, string actorName, DateTime now)
    {
        if (service.Scope != "customer") throw new ArgumentException("Serviço interno não possui vínculo de cliente.", nameof(service));
        if (service.Status == "closed") throw new ArgumentException("Serviço encerrado não recebe vínculos.", nameof(service));
        if (customer.DeletedAt.HasValue) throw new ArgumentException("Cliente removido.", nameof(customer));
        if (actor == Guid.Empty) throw new ArgumentException("Operador obrigatório.", nameof(actor));
        if (string.IsNullOrWhiteSpace(actorName)) throw new ArgumentException("Operador obrigatório.", nameof(actorName));
        if (actorName.Length > 120) throw new ArgumentException("Operador inválido.", nameof(actorName));
        return new ManagedServiceCustomer
        {
            ExternalId = Guid.NewGuid(), ManagedService = service, ManagedServiceId = service.Id,
            Customer = customer, CustomerId = customer.Id, StartedAt = now, StartedByUserId = actor, StartedByName = actorName, CreatedAt = now, ActorUserId = actor
        };
    }
    public void Close(Guid actor, string actorName, DateTime now)
    {
        if (EndedAt.HasValue) return;
        if (now < StartedAt) throw new ArgumentException("Data anterior ao início.", nameof(now));
        if (actor == Guid.Empty) throw new ArgumentException("Operador obrigatório.", nameof(actor));
        if (string.IsNullOrWhiteSpace(actorName)) throw new ArgumentException("Operador obrigatório.", nameof(actorName));
        if (actorName.Length > 120) throw new ArgumentException("Operador inválido.", nameof(actorName));
        EndedAt = now; EndedByUserId = actor; EndedByName = actorName; UpdatedAt = now; ActorUserId = actor;
    }
}
