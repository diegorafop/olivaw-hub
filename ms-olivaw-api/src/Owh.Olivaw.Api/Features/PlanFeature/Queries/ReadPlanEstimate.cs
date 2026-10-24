using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PlanFeature.Queries;
public sealed class ReadPlanEstimateMediatorQuery : IRequest<ReadPlanEstimateResponse> { public ReadPlanEstimateQuery Payload { get; set; } = new(); }
public sealed class ReadPlanEstimateHandler(IRepository<CostReference> costs) : IRequestHandler<ReadPlanEstimateMediatorQuery, ReadPlanEstimateResponse>
{
    public async Task<ReadPlanEstimateResponse> Handle(ReadPlanEstimateMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var ids = request.Payload.Lines.Select(item => item.CostReferenceExternalId).ToArray();
        var selected = (await costs.GetAsync(item => ids.Contains(item.ExternalId) && item.DeletedAt == null, token)).ToDictionary(item => item.ExternalId);
        if (selected.Count != ids.Length) throw new ValidationException("Confira as origens de custo selecionadas.");
        var lines = request.Payload.Lines.Select(line => new PlanCostLine { Name = selected[line.CostReferenceExternalId].Name, MonthlyCost = selected[line.CostReferenceExternalId].MonthlyCost, AllocationPercent = line.AllocationPercent }).ToList();
        if (lines.Select(item => item.Name.ToLowerInvariant()).Distinct().Count() != lines.Count) throw new ValidationException("Selecione uma versão por origem de custo.");
        var p = request.Payload;
        var result = PlanEstimate.Calculate(lines, p.SupportHours, p.HourlyCost, p.ReservePercent, p.MarginPercent);
        return new() { KnownMonthlyCost = result.KnownMonthlyCost, UnallocatedMonthlyCost = result.UnallocatedMonthlyCost, SuggestedMonthlyPrice = result.SuggestedMonthlyPrice, MissingCosts = result.MissingCosts };
    }
}
public sealed class ReadPlanEstimateValidator : AbstractValidator<ReadPlanEstimateQuery>
{
    public ReadPlanEstimateValidator()
    {
        RuleFor(item => item.Lines).NotNull().Must(items => items is { Count: > 0 and <= 100 });
        RuleForEach(item => item.Lines).NotNull();
        When(item => item.Lines != null && item.Lines.All(line => line != null), () => {
            RuleFor(item => item.Lines).Must(items => items.Select(line => line.CostReferenceExternalId).Distinct().Count() == items.Count).WithMessage("Selecione cada origem de custo uma vez.");
            RuleFor(item => item.Lines).Must(items => items.All(line => line.CostReferenceExternalId != Guid.Empty && line.AllocationPercent >= 0 && line.AllocationPercent <= 100 && decimal.Round(line.AllocationPercent, 2) == line.AllocationPercent));
        });
        RuleFor(item => item.SupportHours).PrecisionScale(18, 2, true);
        RuleFor(item => item.HourlyCost).PrecisionScale(18, 2, true);
        RuleFor(item => item.ReservePercent).PrecisionScale(5, 2, true);
        RuleFor(item => item.MarginPercent).PrecisionScale(5, 2, true);
        RuleFor(item => item.SupportHours).InclusiveBetween(0, 1000);
        RuleFor(item => item.HourlyCost).InclusiveBetween(0, 1000000);
        RuleFor(item => item.ReservePercent).InclusiveBetween(0, 100);
        RuleFor(item => item.MarginPercent).GreaterThanOrEqualTo(0).LessThan(100);
    }
}
public sealed class ReadPlanEstimateMediatorValidator : AbstractValidator<ReadPlanEstimateMediatorQuery>
{
    public ReadPlanEstimateMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadPlanEstimateValidator()).OverridePropertyName(string.Empty); }
}
