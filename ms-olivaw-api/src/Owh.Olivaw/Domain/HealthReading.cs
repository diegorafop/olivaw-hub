using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class HealthReading : Entity
{
    public int InventoryResourceId { get; private set; }
    public InventoryResource InventoryResource { get; private set; } = null!;
    public Guid SampleKey { get; private set; }
    public DateTime ObservedAt { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public string Note { get; private set; } = string.Empty;
    public string Outcome { get; private set; } = "source-error";
    public int MaxAgeMinutes { get; private set; } = 15;
    private HealthReading() { }
    public static bool IsOutcome(string value) => value is "success" or "failure" or "timeout" or "source-error";
    public static HealthReading Create(InventoryResource resource, Guid sampleKey, DateTime observedAt, string source, string note, string outcome, int maxAgeMinutes, Guid actor, string actorName, DateTime now)
    {
        if (resource == null) throw new ArgumentNullException(nameof(resource));
        if (sampleKey == Guid.Empty) throw new ArgumentException("Identificador da amostra obrigatório.");
        if (observedAt.Kind != DateTimeKind.Utc || observedAt > now) throw new ArgumentException("Instante inválido.");
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Autor obrigatório.");
        if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Fonte obrigatória.");
        if (source == null || source.Length > 300) throw new ArgumentException("Campo inválido.", nameof(source));
        if (note == null || note.Length > 1000) throw new ArgumentException("Campo inválido.", nameof(note));
        if (outcome == null || outcome.Length > 30) throw new ArgumentException("Campo inválido.", nameof(outcome));
        if (!IsOutcome(outcome)) throw new ArgumentException("Classificação inválida.");
        if (maxAgeMinutes < 1 || maxAgeMinutes > 1440) throw new ArgumentException("Janela inválida.");
        return new() { ExternalId = Guid.NewGuid(), InventoryResource = resource, InventoryResourceId = resource.Id, SampleKey = sampleKey, ObservedAt = observedAt, ActorUserId = actor, ActorName = actorName, CreatedAt = now, Source = source, Note = note, Outcome = outcome, MaxAgeMinutes = maxAgeMinutes };
    }
}
