using System;
namespace Owh.Olivaw.Client.Features.SupportRequestFeature.Commands;
public sealed class UpdateSupportRequestCommand
{
    public long Revision { get; set; }
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
public sealed class UpdateSupportRequestResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
