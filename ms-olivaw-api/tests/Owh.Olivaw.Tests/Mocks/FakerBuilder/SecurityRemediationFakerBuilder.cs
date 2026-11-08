using System;
using System.Text.Json;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Tests.Mocks;
internal static class SecurityRemediationFakerBuilder
{
    internal static SecurityReport Report()
    {
        var report = SecurityReportFakerBuilder.Entity(); report.Id = 91;
        report.CompletedAt = DateTime.UtcNow.AddMinutes(-1); report.ExecutionStatus = "completed";
        report.RawData = JsonSerializer.Serialize(new { result = new {
            findings = new object[] {
                new { ruleId = "host.ssh-permissions", evidence = "authorizedKeys modo 0o622", title = "Permissões" },
                new { ruleId = "docker.socket", evidence = "Traefik", title = "Socket" }
            }, evidence = new { host = new { permissions = new { authorizedKeys = new { mode = 402, symlink = false } } } }
        } });
        return report;
    }
    internal static SecurityRemediation Ticket(string status) => new()
    {
        Id = 1, ExternalId = Guid.NewGuid(), ExecutorUserId = SecurityReportFakerBuilder.Actor,
        SecurityReportId = 91, HostId = "test-host", Status = status,
        LeaseToken = Guid.Parse("b0000000-0000-0000-0000-000000000002"), LeaseExpiresAt = DateTime.UtcNow.AddMinutes(5),
        ApprovalExpiresAt = DateTime.UtcNow.AddHours(1),
        PlanJson = "{\"Action\":\"restrict-permissions\",\"Target\":\"authorizedKeys\",\"ExpectedMode\":402,\"Title\":\"Permissões\",\"Supported\":true}"
    };
}
