using System;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class SecurityReport : Entity
{
    public Guid RunId { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string HostId { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public string ExecutionStatus { get; set; } = string.Empty;
    public string RawData { get; set; } = string.Empty;
}
