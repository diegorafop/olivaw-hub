using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Repositories;
// Compound date/ID pagination is not available in the generic repository contract.
public interface IRepositoryDeliveryNote : IRepository<DeliveryNote>
{
    Task<IReadOnlyList<DeliveryNote>> ReadPublishedAsync(DeliveryTimelineFilter filter, CancellationToken token);
}
public sealed class DeliveryTimelineFilter
{
    public int CustomerId { get; set; } public Guid? ServiceExternalId { get; set; } public string Category { get; set; } = string.Empty;
    public DateTime? FromAt { get; set; } public DateTime? ToAt { get; set; } public DateTime? BeforeAt { get; set; } public int BeforeId { get; set; }
    public DateTime Now { get; set; }
}
