using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class UsageReading : Entity
{
    public int InventoryResourceId { get; private set; }
    public InventoryResource InventoryResource { get; private set; } = null!;
    public Guid SampleKey { get; private set; }
    public DateTime ObservedAt { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public string Note { get; private set; } = string.Empty;
    public string Metric { get; private set; } = "storage-gib";
    public decimal Value { get; private set; } = 0;
    public string Quality { get; private set; } = "manual";
    private UsageReading() { }
    public static bool IsMetric(string value) => value is "storage-gib" or "memory-gib";
    public static bool IsQuality(string value) => value is "manual" or "measured" or "estimated";
    public static UsageReading Create(InventoryResource resource, Guid sampleKey, DateTime observedAt, string source, string note, string metric, decimal value, string quality, Guid actor, string actorName, DateTime now)
    {
        if (resource == null) throw new ArgumentNullException(nameof(resource));
        if (sampleKey == Guid.Empty) throw new ArgumentException("Identificador da amostra obrigatório.");
        if (observedAt.Kind != DateTimeKind.Utc || observedAt > now) throw new ArgumentException("Instante inválido.");
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Autor obrigatório.");
        if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Fonte obrigatória.");
        if (source == null || source.Length > 300) throw new ArgumentException("Campo inválido.", nameof(source));
        if (note == null || note.Length > 1000) throw new ArgumentException("Campo inválido.", nameof(note));
        if (metric == null || metric.Length > 30) throw new ArgumentException("Campo inválido.", nameof(metric));
        if (quality == null || quality.Length > 20) throw new ArgumentException("Campo inválido.", nameof(quality));
        if (!IsMetric(metric)) throw new ArgumentException("Classificação inválida.");
        if (!IsQuality(quality)) throw new ArgumentException("Classificação inválida.");
        if (value < 0 || value > 1000000000 || decimal.Round(value, 3) != value) throw new ArgumentException("Valor inválido.");
        return new() { ExternalId = Guid.NewGuid(), InventoryResource = resource, InventoryResourceId = resource.Id, SampleKey = sampleKey, ObservedAt = observedAt, ActorUserId = actor, ActorName = actorName, CreatedAt = now, Source = source, Note = note, Metric = metric, Value = value, Quality = quality };
    }
}
