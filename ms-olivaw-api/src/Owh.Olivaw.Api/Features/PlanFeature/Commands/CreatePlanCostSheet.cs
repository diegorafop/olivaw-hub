using FluentValidation;
using MediatR;
using System.Security.Cryptography;
using System.Text.Json;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
namespace Owh.Olivaw.Api.Features.PlanFeature.Commands;
public sealed class CreatePlanCostSheetMediatorCommand:IRequest<CreatePlanCostSheetResult>{public CreatePlanCostSheetCommand Payload{get;set;}=new();public Guid ActorUserId{get;set;}public string ActorName{get;set;}=string.Empty;}
public sealed class CreatePlanCostSheetResult{public string? Conflict{get;set;}public CreatePlanCostSheetResponse Response{get;set;}=new();}
public sealed class CreatePlanCostSheetHandler(IRepository<PlanStudy> studies,IRepository<CostReference> references):IRequestHandler<CreatePlanCostSheetMediatorCommand,CreatePlanCostSheetResult>
{
 public static readonly string[] Keys={"vm","storage","network","backup","domain","tools","licenses","other"};
 public async Task<CreatePlanCostSheetResult> Handle(CreatePlanCostSheetMediatorCommand request,CancellationToken token)
 {
  ValidationHelper.ThrowIfNull(request);var p=request.Payload;var digest=Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(p)));
  var existing=await studies.GetFirstOrDefaultAsync(x=>x.ScenarioKey==p.ScenarioKey,token);if(existing!=null)return existing.ScenarioDigest==digest?new(){Response=Response(existing)}:new(){Conflict="Chave já utilizada por outro estudo. Confira o histórico antes de salvar outro cenário."};
  var now=DateTime.UtcNow;var costs=new List<(CostReference Cost,decimal Allocation)>();
  foreach(var line in p.Lines)
  {
   var monthly=decimal.Round(line.Amount/(line.Period=="annual"?12:1),2,MidpointRounding.AwayFromZero);
   var cost=CostReference.Create(p.PlanKey+" / "+line.Key,monthly,p.Source,now,request.ActorUserId,request.ActorName,now);
   await references.AddAsync(cost,token);
   costs.Add((cost,decimal.Round(100m/line.SharedClients,2,MidpointRounding.AwayFromZero)));
  }
  var study=PlanStudy.Create(p.PlanKey+" — composição de custos",p.IncludedFeatures,p.StorageLimitGiB.HasValue?decimal.Round(p.StorageLimitGiB.Value*1.073741824m,3):null,p.MemoryLimitMiB,costs,p.SupportHours,p.HourlyCost,p.ReservePercent,p.MarginPercent,request.ActorUserId,request.ActorName,now,p.CommercialPercent);study.IdentifyScenario(p.ScenarioKey,digest,JsonSerializer.Serialize(p,new JsonSerializerOptions(JsonSerializerDefaults.Web)));
  await studies.AddAsync(study,token);if(!await InventoryRules.SaveAsync(studies,token))return new(){Conflict="Estudo salvo em outro envio. Consulte antes de repetir."};return new(){Response=Response(study)};
 }
 private static CreatePlanCostSheetResponse Response(PlanStudy study)=>new(){ExternalId=study.ExternalId,MonthlyCostWithReserve=decimal.Round((study.KnownMonthlyCost+study.SupportHours*study.HourlyCost)*(1+study.ReservePercent/100),2,MidpointRounding.AwayFromZero),SuggestedMonthlyPrice=study.SuggestedMonthlyPrice,UnallocatedMonthlyCost=study.UnallocatedMonthlyCost};
}
public sealed class CreatePlanCostSheetValidator:AbstractValidator<CreatePlanCostSheetCommand>
{
 public CreatePlanCostSheetValidator()
 {
  RuleFor(x=>x.ScenarioKey).NotEmpty();RuleFor(x=>x.PlanKey).Must(PlanVersion.IsPlan);RuleFor(x=>x.Source).NotEmpty().MaximumLength(300);RuleFor(x=>x.IncludedFeatures).NotEmpty().MaximumLength(3000);
  RuleFor(x=>x.StorageLimitGiB).GreaterThan(0).LessThanOrEqualTo(100000000).PrecisionScale(18,3,true);RuleFor(x=>x.MemoryLimitMiB).GreaterThan(0).LessThanOrEqualTo(1000000000).PrecisionScale(18,3,true);
  RuleFor(x=>x.SupportHours).InclusiveBetween(0,1000).PrecisionScale(18,2,true);RuleFor(x=>x.HourlyCost).InclusiveBetween(0,1000000).PrecisionScale(18,2,true);RuleFor(x=>x.ReservePercent).InclusiveBetween(0,100).PrecisionScale(5,2,true);RuleFor(x=>x.MarginPercent).InclusiveBetween(0,99.99m).PrecisionScale(5,2,true);RuleFor(x=>x.CommercialPercent).InclusiveBetween(0,99.99m).PrecisionScale(5,2,true);RuleFor(x=>x).Must(p=>p.MarginPercent+p.CommercialPercent<100).WithName("CommercialPercent");
  RuleFor(x=>x.Lines).NotNull().Must(v=>v!=null && v.Count==8 && v.All(x=>x!=null) && v.Select(x=>x.Key).OrderBy(x=>x).SequenceEqual(CreatePlanCostSheetHandler.Keys.OrderBy(x=>x)));
  RuleForEach(x=>x.Lines).NotNull().ChildRules(line=>{line.RuleFor(x=>x.Key).Must(v=>CreatePlanCostSheetHandler.Keys.Contains(v));line.RuleFor(x=>x.Amount).InclusiveBetween(0,100000000).PrecisionScale(18,2,true);line.RuleFor(x=>x.Period).Must(v=>v is "monthly" or "annual");line.RuleFor(x=>x.SharedClients).InclusiveBetween(1,1000);});
 }
}
public sealed class CreatePlanCostSheetMediatorValidator:AbstractValidator<CreatePlanCostSheetMediatorCommand>{public CreatePlanCostSheetMediatorValidator(){RuleFor(x=>x.ActorUserId).NotEmpty();RuleFor(x=>x.ActorName).NotEmpty().MaximumLength(120);RuleFor(x=>x.Payload).NotNull().SetValidator(new CreatePlanCostSheetValidator()).OverridePropertyName(string.Empty);}}
