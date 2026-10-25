using System.Text.Json;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Client.Features.SecurityRemediationFeature.Queries;
namespace Owh.Olivaw.Api.Features.SecurityRemediationFeature.Internals;
internal sealed record RemediationPlan(string Action, string Target, int ExpectedMode, string Title, bool Supported);
internal static class RemediationRules
{
    internal static RemediationPlan Plan(SecurityReport report, int index)
    {
        try { return BuildPlan(report, index); }
        catch (Exception error) when (error is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        { return new("manual", "", 0, "Formato sem procedimento automático seguro; revisar manualmente", false); }
    }
    private static RemediationPlan BuildPlan(SecurityReport report, int index)
    {
        using var document = JsonDocument.Parse(report.RawData);
        var root = document.RootElement;
        var findings = root.GetProperty("result").GetProperty("findings");
        if (findings.ValueKind != JsonValueKind.Array || index < 0 || index >= findings.GetArrayLength())
            throw new FluentValidation.ValidationException("Verificação não encontrada.");
        var finding = findings[index];
        var rule = finding.GetProperty("ruleId").GetString();
        var target = "";
        if (rule == "host.ssh-permissions")
        {
            var evidence = finding.GetProperty("evidence").GetString() ?? "";
            if (evidence.StartsWith("sshDirectory modo ", StringComparison.Ordinal)) target = "sshDirectory";
            if (evidence.StartsWith("authorizedKeys modo ", StringComparison.Ordinal)) target = "authorizedKeys";
        }
        if (rule == "host.socket-permissions") target = "dockerSocket";
        if (target.Length > 0 && root.GetProperty("result").TryGetProperty("evidence", out var evidenceRoot) &&
            evidenceRoot.TryGetProperty("host", out var host) && host.TryGetProperty("permissions", out var permissions) &&
            permissions.TryGetProperty(target, out var metadata) && metadata.TryGetProperty("mode", out var mode) && mode.TryGetInt32(out var value) &&
            value >= 0 && value <= 4095 && metadata.TryGetProperty("symlink", out var symlink) && symlink.ValueKind == JsonValueKind.False)
        {
            return new("restrict-permissions", target, value, "Restringir permissões de " + target + "; validar infraestrutura antes e depois", true);
        }
        return new("manual", "", 0, "Procedimento automático seguro ainda não disponível para esta verificação", false);
    }
    internal static SecurityRemediationItem Item(SecurityRemediation entity)
    {
        var plan = JsonSerializer.Deserialize<RemediationPlan>(entity.PlanJson)!;
        var status = entity.Status;
        var message = entity.Message;
        if (status == "authorized" && entity.ApprovalExpiresAt <= DateTime.UtcNow)
        {
            status = "intervention"; message = "Autorização expirada; gere uma análise atualizada.";
        }
        if (status is "executing" or "validating" && entity.LeaseExpiresAt <= DateTime.UtcNow)
        {
            status = "intervention"; message = "Execução sem confirmação no prazo; revisar antes de qualquer nova tentativa.";
        }
        return new() { ExternalId = entity.ExternalId, FindingIndex = entity.FindingIndex, HostId = entity.HostId,
            Status = status, Message = message, PlanJson = entity.PlanJson, PlanTitle = plan.Title,
            Supported = plan.Supported, ApprovalExpiresAt = entity.ApprovalExpiresAt };
    }
    internal static SecurityRemediation Create(SecurityReport report, int index, Guid actor)
    {
        var plan = Plan(report, index);
        var entity = new SecurityRemediation
        {
            ExternalId = Guid.NewGuid(), SecurityReportId = report.Id, FindingIndex = index,
            ExecutorUserId = report.ActorUserId, HostId = report.HostId, ActorUserId = actor,
            CreatedAt = DateTime.UtcNow, ApprovalExpiresAt = DateTime.UtcNow.AddHours(24),
            PlanJson = JsonSerializer.Serialize(plan), Status = "intervention", Message = plan.Title
        };
        if (plan.Supported && report.CompletedAt > DateTime.UtcNow.AddHours(-24) && report.ExecutionStatus != "failed")
        {
            entity.Status = "authorized"; entity.Message = "Autorizado pelo operador; aguardando Baley.";
        }
        else if (plan.Supported) entity.Message = "Relatório antigo ou coleta falhou; gere uma análise atualizada.";
        return entity;
    }
    internal static bool HealthProof(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 16 });
            var root = document.RootElement;
            return root.GetProperty("healthBefore").GetProperty("healthy").ValueKind == JsonValueKind.True &&
                root.GetProperty("healthAfter").GetProperty("healthy").ValueKind == JsonValueKind.True &&
                root.GetProperty("findingAbsent").ValueKind == JsonValueKind.True &&
                root.GetProperty("snapshotUnchanged").ValueKind == JsonValueKind.True &&
                root.GetProperty("healthAfter").GetProperty("samples").GetInt32() >= 2;
        }
        catch (Exception error) when (error is JsonException or KeyNotFoundException or InvalidOperationException or FormatException) { return false; }
    }
}
