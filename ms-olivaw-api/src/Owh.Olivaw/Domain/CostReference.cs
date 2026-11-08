using System;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class CostReference : Entity
{
    public string Name { get; private set; } = string.Empty;
    public decimal? MonthlyCost { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public string Source { get; private set; } = string.Empty;
    public DateTime EffectiveAt { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    private CostReference() { }
    public static CostReference Create(string name, decimal? monthlyCost, string source, DateTime effectiveAt, Guid actor, string actorName, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 120) throw new ArgumentException("Nome inválido.");
        if (monthlyCost < 0 || monthlyCost > 1000000000) throw new ArgumentException("Custo inválido.");
        if (monthlyCost.HasValue && decimal.Round(monthlyCost.Value, 2) != monthlyCost.Value) throw new ArgumentException("Use duas casas decimais para o custo.");
        if (string.IsNullOrWhiteSpace(source) || source.Length > 300) throw new ArgumentException("Fonte obrigatória.");
        if (effectiveAt.Kind != DateTimeKind.Utc || effectiveAt > now) throw new ArgumentException("Vigência inválida.");
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Autor inválido.");
        return new() { ExternalId = Guid.NewGuid(), Name = name, MonthlyCost = monthlyCost, Source = source, EffectiveAt = effectiveAt, ActorUserId = actor, ActorName = actorName, CreatedAt = now };
    }
}
