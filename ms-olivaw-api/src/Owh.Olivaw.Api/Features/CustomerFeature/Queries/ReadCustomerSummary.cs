using MediatR;
using Owh.Olivaw.Client.Features.CustomerFeature.Queries;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;

namespace Owh.Olivaw.Api.Features.CustomerFeature.Queries;

public sealed class ReadCustomerSummaryMediatorQuery : IRequest<ReadCustomerSummaryResponse> { }
public sealed class ReadCustomerSummaryHandler(IRepositoryCustomer repository) : IRequestHandler<ReadCustomerSummaryMediatorQuery, ReadCustomerSummaryResponse>
{
    public async Task<ReadCustomerSummaryResponse> Handle(ReadCustomerSummaryMediatorQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var counts = await repository.ReadCountsAsync(cancellationToken);
        return new ReadCustomerSummaryResponse
        {
            Total = counts.Values.Sum(), Onboarding = counts.GetValueOrDefault("onboarding"), Active = counts.GetValueOrDefault("active"),
            Paused = counts.GetValueOrDefault("paused"), Closed = counts.GetValueOrDefault("closed"), AsOf = DateTimeOffset.UtcNow
        };
    }
}
