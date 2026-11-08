using System;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class CustomerPlan : Entity
{
    public int? PlanVersionId { get; private set; }
    public PlanVersion? PlanVersion { get; private set; }
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public decimal MonthlyPrice { get; private set; }
    public decimal? MemoryLimitGiB { get; private set; }
    public decimal? StorageLimitGiB { get; private set; }
    public string Conditions { get; private set; } = string.Empty;
    public string ApprovalReference { get; private set; } = string.Empty;
    public string ActorName { get; private set; } = string.Empty;
    public DateTime StartsAt { get; private set; }
    private CustomerPlan() { }
    public static CustomerPlan Assign(Customer customer, PlanVersion version, decimal? price, DateTime startsAt, string approval, Guid actor, string actorName, DateTime now)
    {
        if(startsAt.Day!=1 || startsAt.TimeOfDay!=TimeSpan.Zero || startsAt < version.EffectiveAt) throw new ArgumentException("Competência anterior à versão ou não mensal.");
        var entity=Create(customer,char.ToUpperInvariant(version.PlanKey[0])+version.PlanKey.Substring(1),price ?? version.MonthlyPrice,version.MemoryLimitMiB/1024m,version.StorageLimitGiB,version.Conditions,approval,startsAt,actor,actorName,now);
        entity.PlanVersion=version;entity.PlanVersionId=version.Id;return entity;
    }
    public static CustomerPlan Create(Customer customer, string name, decimal monthlyPrice, decimal? memory, decimal? storage, string conditions, string approval, DateTime startsAt, Guid actor, string actorName, DateTime now)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));
        if (string.IsNullOrWhiteSpace(name) || name.Length > 120) throw new ArgumentException("Nome inválido.");
        if (monthlyPrice < 0 || monthlyPrice > 1000000000 || decimal.Round(monthlyPrice, 2) != monthlyPrice) throw new ArgumentException("Mensalidade inválida.");
        if (memory < 0 || storage < 0 || memory > 1000000000 || storage > 1000000000) throw new ArgumentException("Capacidade inválida.");
        if (string.IsNullOrWhiteSpace(conditions) || conditions.Length > 3000) throw new ArgumentException("Condições obrigatórias.");
        if (string.IsNullOrWhiteSpace(approval) || approval.Length > 300) throw new ArgumentException("Referência do aceite obrigatória.");
        if (startsAt.Kind != DateTimeKind.Utc) throw new ArgumentException("Vigência deve estar em UTC.");
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Autor inválido.");
        return new() { ExternalId = Guid.NewGuid(), Customer = customer, CustomerId = customer.Id, Name = name, MonthlyPrice = monthlyPrice, MemoryLimitGiB = memory, StorageLimitGiB = storage, Conditions = conditions, ApprovalReference = approval, StartsAt = startsAt, ActorUserId = actor, ActorName = actorName, CreatedAt = now };
    }
}
