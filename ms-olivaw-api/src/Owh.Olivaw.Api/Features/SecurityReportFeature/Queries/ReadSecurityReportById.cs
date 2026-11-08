using FluentValidation;
using MediatR;
using Owh.Olivaw.Client.Features.SecurityReportFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.SecurityReportFeature.Queries;

public sealed class ReadSecurityReportByIdMediatorQuery : IRequest<ReadSecurityReportByIdResponse>
{
    public ReadSecurityReportByIdQuery Payload { get; set; } = new();
}

public sealed class ReadSecurityReportByIdHandler(IRepository<SecurityReport> repository)
    : IRequestHandler<ReadSecurityReportByIdMediatorQuery, ReadSecurityReportByIdResponse>
{
    public async Task<ReadSecurityReportByIdResponse> Handle(ReadSecurityReportByIdMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var row = await repository.GetFirstOrDefaultAsync(item => item.ExternalId == request.Payload.ExternalId && item.DeletedAt == null, token);
        if (row == null) return new();
        return new() { Item = new SecurityReportItem
        {
            ExternalId = row.ExternalId, RunId = row.RunId, AgentId = row.AgentId, HostId = row.HostId,
            ExecutionStatus = row.ExecutionStatus, CreatedAt = row.CreatedAt, CompletedAt = row.CompletedAt, RawData = row.RawData
        } };
    }
}

public sealed class ReadSecurityReportByIdValidator : AbstractValidator<ReadSecurityReportByIdMediatorQuery>
{
    public ReadSecurityReportByIdValidator()
    {
        RuleFor(item => item.Payload).NotNull();
        RuleFor(item => item.Payload.ExternalId).NotEmpty().When(item => item.Payload != null);
    }
}
