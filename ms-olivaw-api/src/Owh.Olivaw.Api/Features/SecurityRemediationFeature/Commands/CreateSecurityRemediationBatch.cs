using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Api.Features.SecurityRemediationFeature.Internals;
using Owh.Olivaw.Client.Features.SecurityRemediationFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.SecurityRemediationFeature.Commands;
public sealed class CreateSecurityRemediationBatchMediatorCommand : IRequest<CreateSecurityRemediationBatchResponse>
{
    public CreateSecurityRemediationBatchCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
}
public sealed class CreateSecurityRemediationBatchHandler(IRepository<SecurityReport> reports, IRepository<SecurityRemediation> repository)
    : IRequestHandler<CreateSecurityRemediationBatchMediatorCommand, CreateSecurityRemediationBatchResponse>
{
    public async Task<CreateSecurityRemediationBatchResponse> Handle(CreateSecurityRemediationBatchMediatorCommand request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var report = await reports.GetFirstOrDefaultAsync(item => item.ExternalId == request.Payload.ReportExternalId && item.DeletedAt == null, token);
        if (report == null) throw new ValidationException("Relatório não encontrado.");
        using var document = JsonDocument.Parse(report.RawData);
        if (!document.RootElement.GetProperty("result").TryGetProperty("findings", out var findings) || findings.ValueKind != JsonValueKind.Array)
            return new();
        if (findings.GetArrayLength() > 1001) throw new ValidationException("Relatório excede o limite para autorização em lote.");
        var existing = (await repository.GetAsync(item => item.SecurityReportId == report.Id, token)).ToList();
        var indices = existing.Select(item => item.FindingIndex).ToHashSet();
        var added = new List<SecurityRemediation>();
        for (var index = 0; index < findings.GetArrayLength(); index++)
            if (!indices.Contains(index)) added.Add(RemediationRules.Create(report, index, request.ActorUserId));
        if (added.Count > 0)
        {
            await repository.AddCollectionAsync(added, token);
            try { await repository.SaveChangesAsync(token); }
            catch (DbUpdateException error) when (error.InnerException is Npgsql.PostgresException { SqlState: "23505" })
            { throw new ValidationException("Autorizações mudaram durante a operação. Atualize os estados antes de tentar novamente."); }
        }
        return new() { AuthorizedCount = added.Count(item => item.Status == "authorized"), InterventionCount = added.Count(item => item.Status == "intervention"), AlreadyHandledCount = existing.Count };
    }
}
public sealed class CreateSecurityRemediationBatchValidator : AbstractValidator<CreateSecurityRemediationBatchMediatorCommand>
{
    public CreateSecurityRemediationBatchValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.Payload).NotNull();
        RuleFor(item => item.Payload.ReportExternalId).NotEmpty().When(item => item.Payload != null);
    }
}
