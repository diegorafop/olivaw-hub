using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class DeliveryNoteChange : Entity
{
    public int DeliveryNoteId { get; private set; } public DeliveryNote DeliveryNote { get; private set; } = null!;
    public long Revision { get; private set; } public DateTime? PublishedAt { get; private set; } public string AuthorName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string InternalNote { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public string Category { get; private set; } = "feature";
    public string Status { get; private set; } = "draft";
    public string ApprovalReference { get; private set; } = string.Empty;
    public string ServiceLabel { get; private set; } = string.Empty;
    public DateTime? DeliveredAt { get; private set; }
    private DeliveryNoteChange() { }
    internal static DeliveryNoteChange Create(DeliveryNote note, string author, DateTime now) => new()
    {
        ExternalId = Guid.NewGuid(), DeliveryNote = note, DeliveryNoteId = note.Id, Revision = note.Revision, PublishedAt = note.PublishedAt, AuthorName = author, ActorUserId = note.ActorUserId, CreatedAt = now,
        Title = note.Title,
        Summary = note.Summary,
        InternalNote = note.InternalNote,
        Version = note.Version,
        Category = note.Category,
        Status = note.Status,
        ApprovalReference = note.ApprovalReference,
        ServiceLabel = note.ServiceLabel,
        DeliveredAt = note.DeliveredAt,
    };
}
