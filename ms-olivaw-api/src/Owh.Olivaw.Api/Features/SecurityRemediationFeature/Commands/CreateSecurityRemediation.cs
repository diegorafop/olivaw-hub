using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Api.Features.SecurityRemediationFeature.Internals;
using Owh.Olivaw.Client.Features.SecurityRemediationFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.SecurityRemediationFeature.Commands;
public sealed class CreateSecurityRemediationMediatorCommand : IRequest<CreateSecurityRemediationResponse>
{
    public CreateSecurityRemediationCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
}
public sealed class CreateSecurityRemediationHandler(IRepository<SecurityReport> reports, IRepository<SecurityRemediation> repository)
    : IRequestHandler<CreateSecurityRemediationMediatorCommand, CreateSecurityRemediationResponse>
{
    public async Task<CreateSecurityRemediationResponse> Handle(CreateSecurityRemediationMediatorCommand request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var report = await reports.GetFirstOrDefaultAsync(item => item.ExternalId == request.Payload.ReportExternalId && item.DeletedAt == null, token);
        if (report == null) throw new ValidationException("Relatório não encontrado.");
        var existing = await repository.GetFirstOrDefaultAsync(item => item.SecurityReportId == report.Id && item.FindingIndex == request.Payload.FindingIndex, token);
        if (existing != null) return new() { ExternalId = existing.ExternalId, Status = RemediationRules.Item(existing).Status };
        var entity = RemediationRules.Create(report, request.Payload.FindingIndex, request.ActorUserId);
        await repository.AddAsync(entity, token);
        try { await repository.SaveChangesAsync(token); }
        catch (DbUpdateException error) when (error.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            existing = await repository.GetFirstOrDefaultAsync(item => item.SecurityReportId == report.Id && item.FindingIndex == request.Payload.FindingIndex, token);
            if (existing == null) throw;
            return new() { ExternalId = existing.ExternalId, Status = RemediationRules.Item(existing).Status };
        }
        return new() { ExternalId = entity.ExternalId, Status = entity.Status };
    }
}
public sealed class CreateSecurityRemediationValidator : AbstractValidator<CreateSecurityRemediationMediatorCommand>
{
    public CreateSecurityRemediationValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.Payload).NotNull();
        When(item => item.Payload != null, () => {
            RuleFor(item => item.Payload.ReportExternalId).NotEmpty(); RuleFor(item => item.Payload.FindingIndex).InclusiveBetween(0, 1000);
        });
    }
}
