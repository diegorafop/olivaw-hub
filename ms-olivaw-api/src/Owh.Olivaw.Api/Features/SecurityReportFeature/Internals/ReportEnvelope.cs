using System.Text.Json;

namespace Owh.Olivaw.Api.Features.SecurityReportFeature.Internals;

internal sealed record ReportEnvelope(Guid RunId, string AgentId, string HostId, string Version,
    DateTime StartedAt, DateTime CompletedAt, string Status)
{
    internal static ReportEnvelope? Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw.Length > 262144) return null;
        try
        {
            using var document = JsonDocument.Parse(raw, new JsonDocumentOptions { MaxDepth = 32 });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;
            // Duplicate envelope keys have ambiguous meaning for downstream consumers.
            if (root.EnumerateObject().GroupBy(item => item.Name).Any(group => group.Count() > 1)) return null;
            var run = root.GetProperty("runId").GetGuid();
            var agent = root.GetProperty("agentId").GetString();
            var host = root.GetProperty("hostId").GetString();
            var version = root.GetProperty("schemaVersion").GetString();
            var status = root.GetProperty("executionStatus").GetString();
            var started = root.GetProperty("startedAt").GetDateTimeOffset();
            var completed = root.GetProperty("completedAt").GetDateTimeOffset();
            if (run == Guid.Empty || agent != "baley") return null;
            if (string.IsNullOrWhiteSpace(host) || host.Length > 128 || host.Any(char.IsControl)) return null;
            if (version != "1.0") return null;
            if (status is not ("completed" or "partial" or "failed")) return null;
            if (started.Offset != TimeSpan.Zero || completed.Offset != TimeSpan.Zero) return null;
            if (completed < started || completed > DateTimeOffset.UtcNow.AddMinutes(5)) return null;
            if (root.GetProperty("result").ValueKind != JsonValueKind.Object) return null;
            return new(run, agent, host, version, started.UtcDateTime, completed.UtcDateTime, status);
        }
        catch (Exception error) when (error is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            return null;
        }
    }
}
