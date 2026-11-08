using System;
using System.Text.Json;
using Owh.Olivaw.Client.Features.SecurityReportFeature.Commands;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Tests.Mocks;
public static class SecurityReportFakerBuilder
{
    public static readonly Guid Actor = Guid.Parse("a0000000-0000-0000-0000-000000000001");
    public static CreateSecurityReportCommand Create() => new() { RawData = JsonSerializer.Serialize(new
    {
        schemaVersion = "1.0", runId = "b0000000-0000-0000-0000-000000000001", agentId = "baley", hostId = "test-host",
        startedAt = "2026-01-01T00:00:00Z", completedAt = "2026-01-01T00:01:00Z", executionStatus = "partial",
        result = new { findings = Array.Empty<object>(), futureField = new { preserved = true } }
    }) };
    public static SecurityReport Entity() => new()
    {
        Id = 10, ExternalId = Guid.NewGuid(), RunId = Guid.Parse("b0000000-0000-0000-0000-000000000001"),
        AgentId = "baley", HostId = "test-host", ActorUserId = Actor, RawData = Create().RawData
    };
}
