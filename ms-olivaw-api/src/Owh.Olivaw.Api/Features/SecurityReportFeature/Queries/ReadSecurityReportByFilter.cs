using FluentValidation;
using MediatR;
using Owh.Olivaw.Client.Features.SecurityReportFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.SecurityReportFeature.Queries;

public sealed class ReadSecurityReportByFilterMediatorQuery : IRequest<ReadSecurityReportByFilterResponse>
{
    public ReadSecurityReportByFilterQuery Payload { get; set; } = new();
}

public sealed class ReadSecurityReportByFilterHandler(IRepository<SecurityReport> repository)
    : IRequestHandler<ReadSecurityReportByFilterMediatorQuery, ReadSecurityReportByFilterResponse>
{
    public async Task<ReadSecurityReportByFilterResponse> Handle(ReadSecurityReportByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var filter = request.Payload;
        var rows = (await repository.TakeOrderedAsync(item => item.DeletedAt == null &&
            (filter.BeforeId == 0 || item.Id < filter.BeforeId) &&
            (filter.HostId == null || item.HostId == filter.HostId), 21, item => item.Id, token)).ToList();
        var response = new ReadSecurityReportByFilterResponse
        {
            Items = rows.Take(20).Select(item => new SecurityReportItem
            {
                ExternalId = item.ExternalId, RunId = item.RunId, AgentId = item.AgentId, HostId = item.HostId,
                ExecutionStatus = item.ExecutionStatus, CreatedAt = item.CreatedAt,
                CompletedAt = item.CompletedAt, RawData = item.RawData
            }).ToList()
        };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        return response;
    }
}

public sealed class ReadSecurityReportByFilterValidator : AbstractValidator<ReadSecurityReportByFilterQuery>
{
    public ReadSecurityReportByFilterValidator()
    {
        RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0);
        RuleFor(item => item.HostId).MaximumLength(128);
    }
}

public sealed class ReadSecurityReportByFilterMediatorValidator : AbstractValidator<ReadSecurityReportByFilterMediatorQuery>
{
    public ReadSecurityReportByFilterMediatorValidator()
    {
        RuleFor(item => item.Payload).NotNull().SetValidator(new ReadSecurityReportByFilterValidator()).OverridePropertyName(string.Empty);
    }
}
