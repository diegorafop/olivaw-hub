using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class SupportRequestChange : Entity
{
    public int SupportRequestId { get; private set; }
    public SupportRequest SupportRequest { get; private set; } = null!;
    public long Revision { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Requester { get; private set; } = string.Empty;
    public string Impact { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    public string NextAction { get; private set; } = string.Empty;
    public string PublicSummary { get; private set; } = string.Empty;
    public string ServiceLabel { get; private set; } = string.Empty;
    public string InternalNote { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    private SupportRequestChange() { }
    internal static SupportRequestChange Create(SupportRequest entity, string actorName, DateTime now) => new()
    {
        ExternalId = Guid.NewGuid(), SupportRequest = entity, SupportRequestId = entity.Id, Revision = entity.Revision, ActorUserId = entity.ActorUserId, ActorName = actorName, CreatedAt = now,
        Title = entity.Title,
        Requester = entity.Requester,
        Impact = entity.Impact,
        Owner = entity.Owner,
        NextAction = entity.NextAction,
        InternalNote = entity.InternalNote,
        PublicSummary = entity.PublicSummary, ServiceLabel = entity.ManagedService?.Name ?? "",
        Category = entity.Category,
        Status = entity.Status,
    };
}
