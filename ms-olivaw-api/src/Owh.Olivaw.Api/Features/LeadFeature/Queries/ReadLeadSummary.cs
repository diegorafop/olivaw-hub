using FluentValidation;
using MediatR;
using Owh.Olivaw.Client.Features.LeadFeature.Queries;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;

namespace Owh.Olivaw.Api.Features.LeadFeature.Queries;

public sealed class ReadLeadSummaryMediatorQuery : IRequest<ReadLeadSummaryResponse>
{
    public ReadLeadSummaryQuery Payload { get; set; } = new();
}
public sealed class ReadLeadSummaryHandler(IRepositoryLead repository) : IRequestHandler<ReadLeadSummaryMediatorQuery, ReadLeadSummaryResponse>
{
    public async Task<ReadLeadSummaryResponse> Handle(ReadLeadSummaryMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var now = DateTimeOffset.UtcNow;
        var since = now.AddDays(-7);
        var result = await repository.ReadSummaryAsync(since.UtcDateTime, cancellationToken);
        return new ReadLeadSummaryResponse { Total = result.Total, Recent = result.Recent, Since = since, AsOf = now };
    }
}
public sealed class ReadLeadSummaryMediatorValidator : AbstractValidator<ReadLeadSummaryMediatorQuery>
{
    public ReadLeadSummaryMediatorValidator() { RuleFor(item => item.Payload).NotNull(); }
}
