using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Commands;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.HealthReadingFeature.Queries;
public sealed class ReadHealthReadingByFilterMediatorQuery : IRequest<ReadHealthReadingByFilterResponse> { public ReadHealthReadingByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadHealthReadingByFilterHandler(IRepository<HealthReading> repository, IRepository<InventoryResource> resources) : IRequestHandler<ReadHealthReadingByFilterMediatorQuery, ReadHealthReadingByFilterResponse>
{
    public async Task<ReadHealthReadingByFilterResponse> Handle(ReadHealthReadingByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var resource = await InventoryRules.RequireAsync(resources, p.ResourceExternalId, token);
        var rows = (await repository.TakeOrderedAsync(item => item.DeletedAt == null && item.InventoryResourceId == resource.Id && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token)).ToList();
        var response = new ReadHealthReadingByFilterResponse { Items = rows.Take(20).Select(Map).ToList() };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        var latest = (await repository.TakeOrderedAsync(item => item.InventoryResourceId == resource.Id, 1, item => item.ObservedAt, token)).SingleOrDefault();
        if (latest != null)
        {
            response.Latest = Map(latest);
            response.State = "attention";
            if (latest.ObservedAt.AddMinutes(latest.MaxAgeMinutes) < DateTime.UtcNow) response.State = "stale";
            else if (latest.Outcome == "success") response.State = "operational";
            else if (latest.Outcome == "source-error") response.State = "source-error";
        }
        return response;
    }
    private static HealthReadingItem Map(HealthReading item) => new() { ExternalId = item.ExternalId, ObservedAt = item.ObservedAt, ActorName = item.ActorName, Source = item.Source, Note = item.Note, Outcome = item.Outcome, MaxAgeMinutes = item.MaxAgeMinutes };
}
public sealed class ReadHealthReadingByFilterValidator : AbstractValidator<ReadHealthReadingByFilterQuery>
{
    public ReadHealthReadingByFilterValidator() { RuleFor(item => item.ResourceExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); }
}
public sealed class ReadHealthReadingByFilterMediatorValidator : AbstractValidator<ReadHealthReadingByFilterMediatorQuery>
{
    public ReadHealthReadingByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadHealthReadingByFilterValidator()).OverridePropertyName(string.Empty); }
}
