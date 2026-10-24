using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PlanFeature.Queries;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PlanFeature.Queries;
public sealed class ReadPlanStudyByFilterMediatorQuery : IRequest<ReadPlanStudyByFilterResponse> { public ReadPlanStudyByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadPlanStudyByFilterHandler(IRepository<PlanStudy> repository, IRepository<PlanStudyCost> studyCosts) : IRequestHandler<ReadPlanStudyByFilterMediatorQuery, ReadPlanStudyByFilterResponse>
{
    public async Task<ReadPlanStudyByFilterResponse> Handle(ReadPlanStudyByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var rows = (await repository.TakeOrderedAsync(item => item.DeletedAt == null && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token)).ToList();
        var ids = rows.Take(20).Select(item => item.Id).ToArray();
        var lines = await studyCosts.GetAsync(item => ids.Contains(item.PlanStudyId), token, item => item.CostReference);
        var response = new ReadPlanStudyByFilterResponse { Items = rows.Take(20).Select(item => new PlanStudyItem { ExternalId = item.ExternalId, Name = item.Name, IncludedFeatures = item.IncludedFeatures, StorageLimitGB = item.StorageLimitGB, MemoryLimitMiB = item.MemoryLimitMiB, SupportHours = item.SupportHours, HourlyCost = item.HourlyCost, ReservePercent = item.ReservePercent, MarginPercent = item.MarginPercent, KnownMonthlyCost = item.KnownMonthlyCost, SuggestedMonthlyPrice = item.SuggestedMonthlyPrice, UnallocatedMonthlyCost = item.UnallocatedMonthlyCost, MissingCosts = item.MissingCosts, ActorName = item.ActorName, CreatedAt = item.CreatedAt , Costs = lines.Where(cost => cost.PlanStudyId == item.Id).Select(cost => new PlanStudyCostItem { CostReferenceExternalId = cost.CostReference.ExternalId, Name = cost.CostReference.Name, MonthlyCost = cost.CostReference.MonthlyCost, AllocationPercent = cost.AllocationPercent, Source = cost.CostReference.Source, EffectiveAt = cost.CostReference.EffectiveAt }).ToList() }).ToList() };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        return response;
    }
}
public sealed class ReadPlanStudyByFilterValidator : AbstractValidator<ReadPlanStudyByFilterQuery> { public ReadPlanStudyByFilterValidator() { RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); } }
public sealed class ReadPlanStudyByFilterMediatorValidator : AbstractValidator<ReadPlanStudyByFilterMediatorQuery> { public ReadPlanStudyByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadPlanStudyByFilterValidator()).OverridePropertyName(string.Empty); } }
