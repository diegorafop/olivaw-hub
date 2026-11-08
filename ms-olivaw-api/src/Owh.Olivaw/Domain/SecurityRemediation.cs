using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class SecurityRemediation : Entity
{
    public int SecurityReportId { get; set; }
    public SecurityReport SecurityReport { get; set; } = null!;
    public int FindingIndex { get; set; }
    public Guid ExecutorUserId { get; set; }
    public string HostId { get; set; } = string.Empty;
    public string? ActiveHostKey { get; set; }
    public string Status { get; set; } = "authorized";
    public string PlanJson { get; set; } = string.Empty;
    public string ResultJson { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long Revision { get; set; } = 1;
    public Guid? LeaseToken { get; set; }
    public DateTime? LeaseExpiresAt { get; set; }
    public DateTime ApprovalExpiresAt { get; set; }
}
