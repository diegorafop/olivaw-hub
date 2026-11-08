using MediatR;
using Owh.Olivaw.Api.Services;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Queries;
namespace Owh.Olivaw.Api.Features.HealthReadingFeature.Queries;
public sealed class ReadHealthCollectionStatusMediatorQuery : IRequest<ReadHealthCollectionStatusResponse> { }
public sealed class ReadHealthCollectionStatusHandler(ScheduledHealthOptions options,HealthCollectionStatus status) : IRequestHandler<ReadHealthCollectionStatusMediatorQuery,ReadHealthCollectionStatusResponse>
{
    public Task<ReadHealthCollectionStatusResponse> Handle(ReadHealthCollectionStatusMediatorQuery request,CancellationToken token)
    {
        Owh.OlivawCore.Api.Helpers.ValidationHelper.ThrowIfNull(request);token.ThrowIfCancellationRequested();var current=status.Read();
        return Task.FromResult(new ReadHealthCollectionStatusResponse{Enabled=options.Enabled,IntervalMinutes=options.IntervalMinutes,RetentionDays=options.RetentionDays,Resources=options.ResourceExternalIds.Length,CompletedAt=current.CompletedAt,Attempted=current.Attempted,Failures=current.Failures});
    }
}
