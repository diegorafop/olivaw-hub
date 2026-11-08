using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PlanFeature.Commands;
public sealed class CreateCustomerPlanMediatorCommand : IRequest<CreateCustomerPlanResult>
{
    public CreateCustomerPlanCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreateCustomerPlanResult { public string? Conflict { get; set; } public CreateCustomerPlanResponse Response { get; set; } = new(); }
public sealed class CreateCustomerPlanHandler(IRepository<CustomerPlan> repository, IRepository<Customer> customers) : IRequestHandler<CreateCustomerPlanMediatorCommand, CreateCustomerPlanResult>
{
    public async Task<CreateCustomerPlanResult> Handle(CreateCustomerPlanMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var p = request.Payload;
        var customer = await InventoryRules.RequireAsync(customers, p.CustomerExternalId, token);
        var month=new DateTime(p.StartsAt.Year,p.StartsAt.Month,1,0,0,0,DateTimeKind.Utc);
        if(await repository.ExistsAsync(x=>x.CustomerId==customer.Id && x.PlanVersionId!=null && x.StartsAt>=month && x.StartsAt<month.AddMonths(1),token)) return new(){Conflict="Cliente já possui uma versão de catálogo nesta competência. Use o histórico de Planos para a próxima alteração."};
        var entity = CustomerPlan.Create(customer, p.Name, p.MonthlyPrice, p.MemoryLimitGiB, p.StorageLimitGiB, p.Conditions, p.ApprovalReference, p.StartsAt, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await repository.AddAsync(entity, token);
        if (!await InventoryRules.SaveAsync(repository, token)) return new() { Conflict = "Já existe uma versão nesta vigência. Confira o histórico." };
        return new() { Response = new() { ExternalId = entity.ExternalId } };
    }
}
public sealed class CreateCustomerPlanValidator : AbstractValidator<CreateCustomerPlanCommand>
{
    public CreateCustomerPlanValidator()
    {
        RuleFor(item => item.Name).NotEmpty().MaximumLength(120);
        RuleFor(item => item.CustomerExternalId).NotEmpty();
        RuleFor(item => item.MonthlyPrice).InclusiveBetween(0, 1000000000).PrecisionScale(18, 2, true);
        RuleFor(item => item.MemoryLimitGiB).InclusiveBetween(0, 1000000000).PrecisionScale(18, 3, true);
        RuleFor(item => item.StorageLimitGiB).InclusiveBetween(0, 1000000000).PrecisionScale(18, 3, true);
        RuleFor(item => item.Conditions).NotEmpty().MaximumLength(3000);
        RuleFor(item => item.ApprovalReference).NotEmpty().MaximumLength(300);
        RuleFor(item => item.StartsAt).Must(value => value.Kind == DateTimeKind.Utc);
    }
}
public sealed class CreateCustomerPlanMediatorValidator : AbstractValidator<CreateCustomerPlanMediatorCommand>
{
    public CreateCustomerPlanMediatorValidator()
    {
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new CreateCustomerPlanValidator()).OverridePropertyName(string.Empty);
    }
}
