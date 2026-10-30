using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PlanFeature.Commands;
public sealed class CreatePlanCostSheetCommand
{
 public Guid ScenarioKey{get;set;}public string PlanKey{get;set;}=string.Empty;public string Source{get;set;}=string.Empty;public string IncludedFeatures{get;set;}=string.Empty;public decimal? StorageLimitGiB{get;set;}public decimal? MemoryLimitMiB{get;set;}
 public decimal SupportHours{get;set;}public decimal HourlyCost{get;set;}public decimal ReservePercent{get;set;}public decimal MarginPercent{get;set;}public decimal CommercialPercent{get;set;}
 public List<PlanCostSheetLine> Lines{get;set;}=new();
}
public sealed class PlanCostSheetLine {public string Key{get;set;}=string.Empty;public decimal Amount{get;set;}public string Period{get;set;}="monthly";public int SharedClients{get;set;}=1;}
public sealed class CreatePlanCostSheetResponse{public Guid ExternalId{get;set;}public decimal MonthlyCostWithReserve{get;set;}public decimal? SuggestedMonthlyPrice{get;set;}public decimal UnallocatedMonthlyCost{get;set;} }
