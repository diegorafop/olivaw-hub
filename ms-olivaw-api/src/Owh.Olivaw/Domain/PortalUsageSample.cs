using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
// Uma amostra diária atribuída explicitamente a uma empresa; nunca o uso agregado de uma VM.
public sealed class PortalUsageSample : Entity
{
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public Guid SampleKey { get; private set; }
    public DateTime Day { get; private set; }
    public decimal? StorageUsedMiB { get; private set; }
    public long? PageViews { get; private set; }
    public string Source { get; private set; } = string.Empty;
    private PortalUsageSample() { }
    public static PortalUsageSample Create(Customer customer, Guid sampleKey, DateTime day, decimal? storage, long? pageViews, string source, Guid actor, DateTime now)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));
        if (sampleKey == Guid.Empty || actor == Guid.Empty || string.IsNullOrWhiteSpace(source) || source.Length > 300) throw new ArgumentException("Origem da amostra obrigatória.");
        if (day.Kind != DateTimeKind.Utc || day.TimeOfDay != TimeSpan.Zero || day >= now.Date) throw new ArgumentException("Informe um dia UTC encerrado.");
        if (storage < 0 || storage > 1000000000 || (storage.HasValue && decimal.Round(storage.Value, 3) != storage) || pageViews < 0 || pageViews > 1000000000000 || (!storage.HasValue && !pageViews.HasValue)) throw new ArgumentException("Métricas inválidas.");
        return new() {ExternalId=Guid.NewGuid(),Customer=customer,CustomerId=customer.Id,SampleKey=sampleKey,Day=day,StorageUsedMiB=storage,PageViews=pageViews,Source=source,ActorUserId=actor,CreatedAt=now};
    }
}
