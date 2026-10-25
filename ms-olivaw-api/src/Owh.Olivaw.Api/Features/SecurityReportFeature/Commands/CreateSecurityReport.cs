using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Api.Features.SecurityReportFeature.Internals;
using Owh.Olivaw.Client.Features.SecurityReportFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.SecurityReportFeature.Commands;

public sealed class CreateSecurityReportMediatorCommand : IRequest<CreateSecurityReportResponse>
{
    public CreateSecurityReportCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
}

public sealed class CreateSecurityReportHandler(IRepository<SecurityReport> repository)
    : IRequestHandler<CreateSecurityReportMediatorCommand, CreateSecurityReportResponse>
{
    public async Task<CreateSecurityReportResponse> Handle(CreateSecurityReportMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var envelope = ReportEnvelope.Parse(request.Payload.RawData);
        if (envelope == null) throw new ValidationException("Envelope de relatório inválido.");
        var existing = await repository.GetFirstOrDefaultAsync(item => item.AgentId == envelope.AgentId && item.RunId == envelope.RunId, token);
        if (existing != null) return Existing(existing, request);
        var entity = new SecurityReport
        {
            ExternalId = Guid.NewGuid(), ActorUserId = request.ActorUserId, CreatedAt = DateTime.UtcNow,
            RunId = envelope.RunId, AgentId = envelope.AgentId, HostId = envelope.HostId,
            SchemaVersion = envelope.Version, StartedAt = envelope.StartedAt, CompletedAt = envelope.CompletedAt,
            ExecutionStatus = envelope.Status, RawData = request.Payload.RawData
        };
        await repository.AddAsync(entity, token);
        try
        {
            await repository.SaveChangesAsync(token);
        }
        catch (DbUpdateException error) when (error.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            existing = await repository.GetFirstOrDefaultAsync(item => item.AgentId == envelope.AgentId && item.RunId == envelope.RunId, token);
            if (existing == null) throw;
            return Existing(existing, request);
        }
        return new() { ExternalId = entity.ExternalId, RunId = entity.RunId };
    }

    private static CreateSecurityReportResponse Existing(SecurityReport entity, CreateSecurityReportMediatorCommand request)
    {
        if (entity.ActorUserId != request.ActorUserId || entity.RawData != request.Payload.RawData)
            throw new ValidationException("RunId já utilizado com outro conteúdo ou autor.");
        return new() { ExternalId = entity.ExternalId, RunId = entity.RunId, AlreadyReceived = true };
    }
}

public sealed class CreateSecurityReportValidator : AbstractValidator<CreateSecurityReportCommand>
{
    public CreateSecurityReportValidator()
    {
        RuleFor(item => item.RawData).NotEmpty().MaximumLength(262144).Must(raw => ReportEnvelope.Parse(raw) != null)
            .WithMessage("Relatório JSON inválido ou envelope incompatível.");
    }
}

public sealed class CreateSecurityReportMediatorValidator : AbstractValidator<CreateSecurityReportMediatorCommand>
{
    public CreateSecurityReportMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateSecurityReportValidator()).OverridePropertyName(string.Empty);
    }
}
