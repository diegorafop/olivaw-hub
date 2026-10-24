using System;
using System.Collections.Generic;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class DeliveryNote : Entity
{
    public int CustomerId { get; private set; } public Customer Customer { get; private set; } = null!;
    public int? ManagedServiceId { get; private set; } public ManagedService? ManagedService { get; private set; }
    public int? SupportRequestId { get; private set; } public SupportRequest? SupportRequest { get; private set; }
    public long Revision { get; private set; } public DateTime? PublishedAt { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string InternalNote { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public string Category { get; private set; } = "feature";
    public string Status { get; private set; } = "draft";
    public string ApprovalReference { get; private set; } = string.Empty;
    public string ServiceLabel { get; private set; } = string.Empty;
    public DateTime? DeliveredAt { get; private set; }
    public ICollection<DeliveryNoteChange> Changes { get; private set; } = new List<DeliveryNoteChange>();
    private DeliveryNote() { }
    public static DeliveryNote Create(Customer customer, ManagedService? service, SupportRequest? request, DeliveryNoteData data, Guid actor, string author, DateTime now)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));
        if (data.Status != "draft") throw new ArgumentException("A entrega deve começar em rascunho.");
        if (request != null && request.CustomerId != customer.Id) throw new ArgumentException("Pedido pertence a outra empresa.");
        var note = new DeliveryNote { ExternalId = Guid.NewGuid(), Customer = customer, CustomerId = customer.Id, ManagedService = service, ManagedServiceId = service?.Id, SupportRequest = request, SupportRequestId = request?.Id, CreatedAt = now };
        note.Changes.Add(note.Apply(data, actor, author, now)); return note;
    }
    public DeliveryNoteChange Apply(DeliveryNoteData data, Guid actor, string author, DateTime now)
    {
        data.Validate(now);
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(author) || author.Length > 120) throw new ArgumentException("Autor inválido.");
        if (data.Status == "published" && SupportRequest != null && SupportRequest.Status is not ("delivered" or "closed")) throw new ArgumentException("Confirme a conclusão do pedido antes de publicar sua entrega.");
        if (data.Status == "published" && Status != "published") PublishedAt = now;
        Title = data.Title;
        Summary = data.Summary;
        InternalNote = data.InternalNote;
        Version = data.Version;
        Category = data.Category;
        Status = data.Status;
        ApprovalReference = data.ApprovalReference;
        ServiceLabel = data.ServiceLabel;
        DeliveredAt = data.DeliveredAt;
        ActorUserId = actor; Revision++; if (Revision > 1) UpdatedAt = now;
        return DeliveryNoteChange.Create(this, author, now);
    }
}
