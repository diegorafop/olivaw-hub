using System.Text.Json;
using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.SecurityRemediationFeature.Internals;
using Owh.Olivaw.Client.Features.SecurityRemediationFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.SecurityRemediationFeature.Queries;
public sealed class ReadSecurityRemediationMediatorQuery : IRequest<ReadSecurityRemediationResponse>
{
    public ReadSecurityRemediationQuery Payload { get; set; } = new();
}
public sealed class ReadSecurityRemediationHandler(IRepository<SecurityReport> reports, IRepository<SecurityRemediation> repository)
    : IRequestHandler<ReadSecurityRemediationMediatorQuery, ReadSecurityRemediationResponse>
{
    public async Task<ReadSecurityRemediationResponse> Handle(ReadSecurityRemediationMediatorQuery request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var report = await reports.GetFirstOrDefaultAsync(item => item.ExternalId == request.Payload.ReportExternalId && item.DeletedAt == null, token);
        if (report == null) throw new ValidationException("Relatório não encontrado.");
        var rows = (await repository.GetAsync(item => item.SecurityReportId == report.Id && item.DeletedAt == null, token)).ToList();
        using var document = JsonDocument.Parse(report.RawData);
        if (!document.RootElement.GetProperty("result").TryGetProperty("findings", out var findings) || findings.ValueKind != JsonValueKind.Array) return new();
        var response = new ReadSecurityRemediationResponse();
        for (var index = 0; index < Math.Min(1001, findings.GetArrayLength()); index++)
        {
            var existing = rows.FirstOrDefault(item => item.FindingIndex == index);
            if (existing != null) { response.Items.Add(RemediationRules.Item(existing)); continue; }
            var plan = RemediationRules.Plan(report, index);
            response.Items.Add(new() { FindingIndex = index, HostId = report.HostId, PlanTitle = plan.Title, Supported = plan.Supported });
        }
        return response;
    }
}
public sealed class ReadSecurityRemediationValidator : AbstractValidator<ReadSecurityRemediationMediatorQuery>
{
    public ReadSecurityRemediationValidator() { RuleFor(item => item.Payload).NotNull(); RuleFor(item => item.Payload.ReportExternalId).NotEmpty().When(item => item.Payload != null); }
}
public sealed class ReadSecurityRemediationQueueMediatorQuery : IRequest<ReadSecurityRemediationQueueResponse>
{
    public string HostId { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
}
public sealed class ReadSecurityRemediationQueueHandler(IRepository<SecurityRemediation> repository)
    : IRequestHandler<ReadSecurityRemediationQueueMediatorQuery, ReadSecurityRemediationQueueResponse>
{
    public async Task<ReadSecurityRemediationQueueResponse> Handle(ReadSecurityRemediationQueueMediatorQuery request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var now = DateTime.UtcNow;
        var rows = await repository.TakeOrderedAsync(item => item.ExecutorUserId == request.ActorUserId && item.HostId == request.HostId && item.Status == "authorized" && item.ApprovalExpiresAt > now && item.DeletedAt == null, 5, item => item.Id, token);
        return new() { Items = rows.Select(RemediationRules.Item).ToList() };
    }
}
public sealed class ReadSecurityRemediationQueueValidator : AbstractValidator<ReadSecurityRemediationQueueMediatorQuery>
{
    public ReadSecurityRemediationQueueValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.HostId).NotEmpty().MaximumLength(128); }
}
