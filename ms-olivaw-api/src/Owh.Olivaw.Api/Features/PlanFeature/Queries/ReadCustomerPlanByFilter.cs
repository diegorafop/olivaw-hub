using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PlanFeature.Queries;
public sealed class ReadCustomerPlanByFilterMediatorQuery : IRequest<ReadCustomerPlanByFilterResponse> { public ReadCustomerPlanByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadCustomerPlanByFilterHandler(IRepository<CustomerPlan> repository) : IRequestHandler<ReadCustomerPlanByFilterMediatorQuery, ReadCustomerPlanByFilterResponse>
{
    public async Task<ReadCustomerPlanByFilterResponse> Handle(ReadCustomerPlanByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var p = request.Payload;
        var rows = (await repository.TakeOrderedAsync(item => item.DeletedAt == null && (p.BeforeId == 0 || item.Id < p.BeforeId) && item.Customer.ExternalId == p.CustomerExternalId, 21, item => item.Id, token, item => item.Customer, item => item.PlanVersion!)).ToList();
        var response = new ReadCustomerPlanByFilterResponse { Items = rows.Take(20).Select(Map).ToList() };
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        var current = await repository.TakeOrderedAsync(item => item.DeletedAt == null && item.Customer.ExternalId == p.CustomerExternalId && item.StartsAt <= p.At, 1, item => item.StartsAt, token, item => item.Customer, item => item.PlanVersion!);
        response.Current = current.Select(Map).FirstOrDefault();
        return response;
    }
    private static CustomerPlanItem Map(CustomerPlan item) => new() { ExternalId = item.ExternalId, PlanVersionExternalId=item.PlanVersion?.ExternalId, Version=item.PlanVersion?.Version, Name = item.Name, MonthlyPrice = item.MonthlyPrice, MemoryLimitGiB = item.MemoryLimitGiB, StorageLimitGiB = item.StorageLimitGiB, Conditions = item.Conditions, ApprovalReference = item.ApprovalReference, ActorName = item.ActorName, StartsAt = item.StartsAt };
}
public sealed class ReadCustomerPlanByFilterValidator : AbstractValidator<ReadCustomerPlanByFilterQuery>
{
    public ReadCustomerPlanByFilterValidator() { RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); RuleFor(item => item.CustomerExternalId).NotEmpty();
        RuleFor(item => item.At).Must(value => value.Kind == DateTimeKind.Utc); }
}
public sealed class ReadCustomerPlanByFilterMediatorValidator : AbstractValidator<ReadCustomerPlanByFilterMediatorQuery>
{
    public ReadCustomerPlanByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadCustomerPlanByFilterValidator()).OverridePropertyName(string.Empty); }
}
