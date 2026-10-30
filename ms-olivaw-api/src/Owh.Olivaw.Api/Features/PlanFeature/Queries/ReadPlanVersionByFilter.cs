using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
namespace Owh.Olivaw.Api.Features.PlanFeature.Queries;
public sealed class ReadPlanVersionByFilterMediatorQuery : IRequest<ReadPlanVersionByFilterResponse>{public ReadPlanVersionByFilterQuery Payload {get;set;}=new();}
public sealed class ReadPlanVersionByFilterHandler(IRepository<PlanVersion> versions) : IRequestHandler<ReadPlanVersionByFilterMediatorQuery,ReadPlanVersionByFilterResponse>
{
    public async Task<ReadPlanVersionByFilterResponse> Handle(ReadPlanVersionByFilterMediatorQuery request,CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);var p=request.Payload;var now=DateTime.UtcNow;
        var rows=(await versions.TakeOrderedAsync(x=>x.DeletedAt==null && x.PlanKey==p.PlanKey && (p.BeforeId==0 || x.Id<p.BeforeId),51,x=>x.Id,token,x=>x.PlanStudy!)).ToList();
        var current=(await versions.TakeOrderedAsync(x=>x.DeletedAt==null && x.PlanKey==p.PlanKey && x.EffectiveAt<=now,1,x=>x.EffectiveAt,token,x=>x.PlanStudy!)).FirstOrDefault();
        return new(){Items=rows.Take(50).Select(Map).ToList(),NextBeforeId=rows.Count>50?rows[49].Id:null,Current=current==null?null:Map(current)};
    }
    private static PlanVersionItem Map(PlanVersion x)=>new(){ExternalId=x.ExternalId,PlanKey=x.PlanKey,Version=x.Version,MonthlyPrice=x.MonthlyPrice,StorageLimitGiB=x.StorageLimitGiB,MemoryLimitMiB=x.MemoryLimitMiB,CpuLimit=x.CpuLimit,Conditions=x.Conditions,ApprovalReference=x.ApprovalReference,EffectiveAt=x.EffectiveAt,PlanStudyExternalId=x.PlanStudy?.ExternalId,EstimatedMonthlyCost=x.PlanStudy?.MissingCosts==0 ? decimal.Round((x.PlanStudy.KnownMonthlyCost+x.PlanStudy.SupportHours*x.PlanStudy.HourlyCost)*(1+x.PlanStudy.ReservePercent/100),2,MidpointRounding.AwayFromZero):null,SuggestedMonthlyPrice=x.PlanStudy?.SuggestedMonthlyPrice};
}
public sealed class ReadPlanVersionByFilterValidator : AbstractValidator<ReadPlanVersionByFilterQuery>{public ReadPlanVersionByFilterValidator(){RuleFor(x=>x.PlanKey).Must(PlanVersion.IsPlan);RuleFor(x=>x.BeforeId).GreaterThanOrEqualTo(0);}}
public sealed class ReadPlanVersionByFilterMediatorValidator : AbstractValidator<ReadPlanVersionByFilterMediatorQuery>{public ReadPlanVersionByFilterMediatorValidator(){RuleFor(x=>x.Payload).NotNull().SetValidator(new ReadPlanVersionByFilterValidator()).OverridePropertyName(string.Empty);}}
