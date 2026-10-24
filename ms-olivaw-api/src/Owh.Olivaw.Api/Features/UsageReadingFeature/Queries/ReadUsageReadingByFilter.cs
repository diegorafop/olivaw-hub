using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.UsageReadingFeature.Commands;
using Owh.Olivaw.Client.Features.UsageReadingFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.UsageReadingFeature.Queries;
public sealed class ReadUsageReadingByFilterMediatorQuery : IRequest<ReadUsageReadingByFilterResponse> { public ReadUsageReadingByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadUsageReadingByFilterHandler(IRepository<UsageReading> repository, IRepository<InventoryResource> resources) : IRequestHandler<ReadUsageReadingByFilterMediatorQuery, ReadUsageReadingByFilterResponse>
{
    public async Task<ReadUsageReadingByFilterResponse> Handle(ReadUsageReadingByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var resource = await InventoryRules.RequireAsync(resources, p.ResourceExternalId, token);
        var rows = (await repository.TakeOrderedAsync(item => item.DeletedAt == null && item.InventoryResourceId == resource.Id && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token)).ToList();
        var response = new ReadUsageReadingByFilterResponse { Items = rows.Take(20).Select(Map).ToList() };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        return response;
    }
    private static UsageReadingItem Map(UsageReading item) => new() { ExternalId = item.ExternalId, ObservedAt = item.ObservedAt, ActorName = item.ActorName, Source = item.Source, Note = item.Note, Metric = item.Metric, Value = item.Value, Quality = item.Quality };
}
public sealed class ReadUsageReadingByFilterValidator : AbstractValidator<ReadUsageReadingByFilterQuery>
{
    public ReadUsageReadingByFilterValidator() { RuleFor(item => item.ResourceExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); }
}
public sealed class ReadUsageReadingByFilterMediatorValidator : AbstractValidator<ReadUsageReadingByFilterMediatorQuery>
{
    public ReadUsageReadingByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadUsageReadingByFilterValidator()).OverridePropertyName(string.Empty); }
}
