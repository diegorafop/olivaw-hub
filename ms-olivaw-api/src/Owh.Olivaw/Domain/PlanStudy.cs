using System;
using System.Collections.Generic;
using System.Linq;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class PlanStudy : Entity
{
    public string? CostSheetData { get; private set; }
    public Guid? ScenarioKey { get; private set; }
    public string? ScenarioDigest { get; private set; }
    public decimal CommercialPercent { get; private set; }
    public void IdentifyScenario(Guid key, string digest, string data) { if(ScenarioKey.HasValue || key==Guid.Empty || digest.Length!=64 || string.IsNullOrWhiteSpace(data) || data.Length>16000) throw new ArgumentException("Identificação do cenário inválida.");ScenarioKey=key;ScenarioDigest=digest;CostSheetData=data; }
    public string Name { get; private set; } = string.Empty;
    public string IncludedFeatures { get; private set; } = string.Empty;
    public decimal? StorageLimitGB { get; private set; }
    public decimal? MemoryLimitMiB { get; private set; }
    public decimal SupportHours { get; private set; }
    public decimal HourlyCost { get; private set; }
    public decimal ReservePercent { get; private set; }
    public decimal MarginPercent { get; private set; }
    public decimal KnownMonthlyCost { get; private set; }
    public decimal? SuggestedMonthlyPrice { get; private set; }
    public decimal UnallocatedMonthlyCost { get; private set; }
    public int MissingCosts { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    public ICollection<PlanStudyCost> Costs { get; private set; } = new List<PlanStudyCost>();
    private PlanStudy() { }
    public static PlanStudy Create(string name, string includedFeatures, decimal? storageGB, decimal? memoryMiB, IReadOnlyCollection<(CostReference Cost, decimal Allocation)> costs, decimal supportHours, decimal hourlyCost, decimal reservePercent, decimal marginPercent, Guid actor, string actorName, DateTime now, decimal commercialPercent = 0)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 120) throw new ArgumentException("Nome do cenário obrigatório.");
        if (string.IsNullOrWhiteSpace(includedFeatures) || includedFeatures.Length > 3000) throw new ArgumentException("Descreva as funcionalidades incluídas.");
        if (storageGB < 0 || storageGB > 1000000000 || memoryMiB < 0 || memoryMiB > 1000000000) throw new ArgumentException("Limite inválido.");
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Autor obrigatório.");
        if (costs == null || costs.Count == 0) throw new ArgumentException("Selecione as origens de custo.");
        if (storageGB.HasValue && decimal.Round(storageGB.Value, 3) != storageGB.Value) throw new ArgumentException("Limite de armazenamento inválido.");
        if (memoryMiB.HasValue && decimal.Round(memoryMiB.Value, 3) != memoryMiB.Value) throw new ArgumentException("Limite de memória inválido.");
        if (costs.Select(item => item.Cost.Name.ToLowerInvariant()).Distinct().Count() != costs.Count) throw new ArgumentException("Selecione uma versão por origem de custo.");
        var result = PlanEstimate.Calculate(costs.Select(item => new PlanCostLine { Name = item.Cost.Name, MonthlyCost = item.Cost.MonthlyCost, AllocationPercent = item.Allocation }).ToList(), supportHours, hourlyCost, reservePercent, marginPercent, commercialPercent);
        var study = new PlanStudy { ExternalId = Guid.NewGuid(), Name = name, CommercialPercent = commercialPercent, IncludedFeatures = includedFeatures, StorageLimitGB = storageGB, MemoryLimitMiB = memoryMiB, SupportHours = supportHours, HourlyCost = hourlyCost, ReservePercent = reservePercent, MarginPercent = marginPercent, KnownMonthlyCost = result.KnownMonthlyCost, SuggestedMonthlyPrice = result.SuggestedMonthlyPrice, UnallocatedMonthlyCost = result.UnallocatedMonthlyCost, MissingCosts = result.MissingCosts, ActorUserId = actor, ActorName = actorName, CreatedAt = now };
        foreach (var item in costs) study.Costs.Add(PlanStudyCost.Create(study, item.Cost, item.Allocation, actor, now));
        return study;
    }
}
public sealed class PlanStudyCost : Entity
{
    public int PlanStudyId { get; private set; }
    public PlanStudy PlanStudy { get; private set; } = null!;
    public int CostReferenceId { get; private set; }
    public CostReference CostReference { get; private set; } = null!;
    public decimal AllocationPercent { get; private set; }
    private PlanStudyCost() { }
    internal static PlanStudyCost Create(PlanStudy study, CostReference cost, decimal allocation, Guid actor, DateTime now) => new() { ExternalId = Guid.NewGuid(), PlanStudy = study, CostReference = cost, CostReferenceId = cost.Id, AllocationPercent = allocation, ActorUserId = actor, CreatedAt = now };
}
