using FluentValidation;
using MediatR;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
namespace Owh.Olivaw.Api.Features.PlanFeature.Commands;
public sealed class CreatePlanAssignmentMediatorCommand : IRequest<CreatePlanAssignmentResult> { public CreatePlanAssignmentCommand Payload { get; set; } = new(); public Guid ActorUserId { get; set; } public string ActorName { get; set; } = string.Empty; }
public sealed class CreatePlanAssignmentResult { public string? Conflict { get; set; } public CreatePlanAssignmentResponse Response { get; set; } = new(); }
public sealed class CreatePlanAssignmentHandler(IRepository<CustomerPlan> plans,IRepository<Customer> customers,IRepository<PlanVersion> versions) : IRequestHandler<CreatePlanAssignmentMediatorCommand,CreatePlanAssignmentResult>
{
    public async Task<CreatePlanAssignmentResult> Handle(CreatePlanAssignmentMediatorCommand request,CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);var p=request.Payload;
        var customer=await InventoryRules.RequireAsync(customers,p.CustomerExternalId,token);var version=await InventoryRules.RequireAsync(versions,p.PlanVersionExternalId,token);
        var date=new DateTime(p.Year,p.Month,1,0,0,0,DateTimeKind.Utc);
        InventoryRules.Require(date>=version.EffectiveAt,"Month","Competência anterior à vigência desta versão.");
        var existing=await plans.GetFirstOrDefaultAsync(x=>x.CustomerId==customer.Id && x.StartsAt>=date && x.StartsAt<date.AddMonths(1),token);
        if(existing!=null) return existing.PlanVersionId==version.Id && existing.MonthlyPrice==(p.AgreedMonthlyPrice ?? version.MonthlyPrice) && existing.ApprovalReference==p.ApprovalReference ? new(){Response=new(){ExternalId=existing.ExternalId}}:new(){Conflict="Cliente já possui uma condição nesta competência. Preserve o histórico e revise o próximo mês."};
        var entity=CustomerPlan.Assign(customer,version,p.AgreedMonthlyPrice,date,p.ApprovalReference,request.ActorUserId,request.ActorName,DateTime.UtcNow);
        await plans.AddAsync(entity,token);if(!await InventoryRules.SaveAsync(plans,token))return new(){Conflict="Condição já registrada nesta competência. Confira o histórico."};
        return new(){Response=new(){ExternalId=entity.ExternalId}};
    }
}
public sealed class CreatePlanAssignmentValidator : AbstractValidator<CreatePlanAssignmentCommand>{public CreatePlanAssignmentValidator(){RuleFor(x=>x.CustomerExternalId).NotEmpty();RuleFor(x=>x.PlanVersionExternalId).NotEmpty();RuleFor(x=>x.Year).InclusiveBetween(2000,2100);RuleFor(x=>x.Month).InclusiveBetween(1,12);RuleFor(x=>x.AgreedMonthlyPrice).InclusiveBetween(0,100000000).PrecisionScale(18,2,true);RuleFor(x=>x.ApprovalReference).NotEmpty().MaximumLength(300);}}
public sealed class CreatePlanAssignmentMediatorValidator : AbstractValidator<CreatePlanAssignmentMediatorCommand>{public CreatePlanAssignmentMediatorValidator(){RuleFor(x=>x.ActorUserId).NotEmpty();RuleFor(x=>x.ActorName).NotEmpty().MaximumLength(120);RuleFor(x=>x.Payload).NotNull().SetValidator(new CreatePlanAssignmentValidator()).OverridePropertyName(string.Empty);}}
