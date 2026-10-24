using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PlanFeature.Queries;
public sealed class ReadCostReferenceByFilterMediatorQuery : IRequest<ReadCostReferenceByFilterResponse> { public ReadCostReferenceByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadCostReferenceByFilterHandler(IRepository<CostReference> repository) : IRequestHandler<ReadCostReferenceByFilterMediatorQuery, ReadCostReferenceByFilterResponse>
{
    public async Task<ReadCostReferenceByFilterResponse> Handle(ReadCostReferenceByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var p = request.Payload;
        var rows = (await repository.TakeOrderedAsync(item => item.DeletedAt == null && (p.BeforeId == 0 || item.Id < p.BeforeId) && (p.Search == null || item.Name.Contains(p.Search)), 21, item => item.Id, token)).ToList();
        var response = new ReadCostReferenceByFilterResponse { Items = rows.Take(20).Select(Map).ToList() };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        return response;
    }
    private static CostReferenceItem Map(CostReference item) => new() { ExternalId = item.ExternalId, Name = item.Name, MonthlyCost = item.MonthlyCost, Source = item.Source, EffectiveAt = item.EffectiveAt, ActorName = item.ActorName };
}
public sealed class ReadCostReferenceByFilterValidator : AbstractValidator<ReadCostReferenceByFilterQuery>
{
    public ReadCostReferenceByFilterValidator() { RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); RuleFor(item => item.Search).MaximumLength(120); }
}
public sealed class ReadCostReferenceByFilterMediatorValidator : AbstractValidator<ReadCostReferenceByFilterMediatorQuery>
{
    public ReadCostReferenceByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadCostReferenceByFilterValidator()).OverridePropertyName(string.Empty); }
}
