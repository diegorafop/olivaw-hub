using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PlanFeature.Queries;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PlanFeature.Commands;
public sealed class CreatePlanStudyMediatorCommand : IRequest<CreatePlanStudyResult>
{
    public CreatePlanStudyCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreatePlanStudyResult { public string? Conflict { get; set; } public CreatePlanStudyResponse Response { get; set; } = new(); }
public sealed class CreatePlanStudyHandler(IRepository<PlanStudy> repository, IRepository<CostReference> costs) : IRequestHandler<CreatePlanStudyMediatorCommand, CreatePlanStudyResult>
{
    public async Task<CreatePlanStudyResult> Handle(CreatePlanStudyMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var ids = p.Lines.Select(item => item.CostReferenceExternalId).ToArray();
        var references = (await costs.GetAsync(item => ids.Contains(item.ExternalId) && item.DeletedAt == null, token)).ToDictionary(item => item.ExternalId);
        if (references.Count != ids.Length) throw new ValidationException("Confira as origens de custo selecionadas.");
        var selected = p.Lines.Select(line => (Cost: references[line.CostReferenceExternalId], Allocation: line.AllocationPercent)).ToList();
        if (selected.Select(item => item.Cost.Name.ToLowerInvariant()).Distinct().Count() != selected.Count) return new() { Conflict = "Selecione uma versão por origem de custo." };
        var study = PlanStudy.Create(p.Name, p.IncludedFeatures, p.StorageLimitGB, p.MemoryLimitMiB, selected, p.SupportHours, p.HourlyCost, p.ReservePercent, p.MarginPercent, request.ActorUserId, request.ActorName, DateTime.UtcNow, p.CommercialPercent);
        await repository.AddAsync(study, token); await repository.SaveChangesAsync(token);
        return new() { Response = new() { ExternalId = study.ExternalId } };
    }
}
public sealed class CreatePlanStudyValidator : AbstractValidator<CreatePlanStudyCommand>
{
    public CreatePlanStudyValidator()
    {
        RuleFor(item => item.Name).NotEmpty().MaximumLength(120);
        RuleFor(item => item.IncludedFeatures).NotEmpty().MaximumLength(3000);
        RuleFor(item => item.StorageLimitGB).InclusiveBetween(0, 1000000000).PrecisionScale(18, 3, true);
        RuleFor(item => item.MemoryLimitMiB).InclusiveBetween(0, 1000000000).PrecisionScale(18, 3, true);
        RuleFor(item => new ReadPlanEstimateQuery { Lines = item.Lines, SupportHours = item.SupportHours, HourlyCost = item.HourlyCost, ReservePercent = item.ReservePercent, MarginPercent = item.MarginPercent, CommercialPercent=item.CommercialPercent }).SetValidator(new ReadPlanEstimateValidator()).OverridePropertyName(string.Empty);
    }
}
public sealed class CreatePlanStudyMediatorValidator : AbstractValidator<CreatePlanStudyMediatorCommand>
{
    public CreatePlanStudyMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreatePlanStudyValidator()).OverridePropertyName(string.Empty);
    }
}
