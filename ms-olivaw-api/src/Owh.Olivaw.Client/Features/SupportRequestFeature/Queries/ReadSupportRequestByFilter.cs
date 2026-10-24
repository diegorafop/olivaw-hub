using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.SupportRequestFeature.Queries;
public sealed class ReadSupportRequestByFilterQuery
{
    public Guid? CustomerExternalId { get; set; }
    public string? Status { get; set; }
    public int BeforeId { get; set; }
}
public sealed class ReadSupportRequestByFilterResponse { public List<SupportRequestItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class SupportRequestItem
{
    public Guid ExternalId { get; set; }
    public Guid? ServiceExternalId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public Guid CustomerExternalId { get; set; }
    public long Revision { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Requester { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string NextAction { get; set; } = string.Empty;
    public string PublicSummary { get; set; } = string.Empty;
    public string InternalNote { get; set; } = string.Empty;
    public string Category { get; set; } = "question";
    public string Status { get; set; } = "received";
}
