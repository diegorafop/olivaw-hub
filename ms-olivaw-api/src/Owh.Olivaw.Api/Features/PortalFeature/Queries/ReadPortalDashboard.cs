using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Queries;
public sealed class ReadPortalDashboardMediatorQuery : IRequest<ReadPortalDashboardResponse> { public ReadPortalDashboardQuery Payload { get; set; } = new(); public Guid ActorUserId { get; set; } public bool Preview { get; set; } }
public sealed class ReadPortalDashboardHandler(PortalAccess access, IRepository<Customer> customers, IRepository<PortalServiceLink> links, IRepositoryHealthReading readings, IRepository<CustomerPlan> plans) : IRequestHandler<ReadPortalDashboardMediatorQuery, ReadPortalDashboardResponse>
{
    public async Task<ReadPortalDashboardResponse> Handle(ReadPortalDashboardMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        Customer customer; var canRequest = false; var canBilling = false;
        if (request.Preview) customer = await InventoryRules.RequireAsync(customers, p.CustomerExternalId, token);
        else { var member = await access.RequireAsync(request.ActorUserId, p.CustomerExternalId, false, false, token); customer = member.Customer; canRequest = member.CanRequest; canBilling = member.CanReadBilling; }
        var rows = (await links.TakeOrderedAsync(item => item.CustomerId == customer.Id && item.IsActive && item.DeletedAt == null && item.ManagedService.Customers.Any(association => association.CustomerId == customer.Id && association.EndedAt == null) && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token, item => item.ManagedService)).ToList();
        var ids = rows.Take(20).Where(item => item.HealthResourceId.HasValue).Select(item => item.HealthResourceId!.Value).Distinct().ToArray();
        var latest = (await readings.ReadLatestAsync(ids, token)).ToDictionary(item => item.InventoryResourceId);
        var now = DateTime.UtcNow;
        var response = new ReadPortalDashboardResponse { CustomerExternalId = customer.ExternalId, Business = customer.Business, Preview = request.Preview, CanRequest = canRequest, CanReadBilling = canBilling };
        foreach (var link in rows.Take(20))
        {
            var item = new PortalServiceItem { ExternalId = link.ExternalId, ServiceExternalId = link.ManagedService.ExternalId, Name = link.Label, PublicUrl = link.PublicUrl };
            if (link.HealthResourceId.HasValue && latest.TryGetValue(link.HealthResourceId.Value, out var reading))
            {
                item.ObservedAt = reading.ObservedAt; item.State = "attention";
                if (reading.ObservedAt.AddMinutes(reading.MaxAgeMinutes) < now) item.State = "stale";
                else if (reading.Outcome == "success") item.State = "available";
                else if (reading.Outcome == "source-error") item.State = "unknown";
            }
            response.Services.Add(item);
        }
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id;
        var plan = (await plans.TakeOrderedAsync(item => item.CustomerId == customer.Id && item.StartsAt <= now && item.DeletedAt == null, 1, item => item.StartsAt, token)).FirstOrDefault();
        if (plan != null) response.CurrentPlan = new() { Name = plan.Name, MonthlyPrice = plan.MonthlyPrice, MemoryLimitGiB = plan.MemoryLimitGiB, StorageLimitGiB = plan.StorageLimitGiB, Conditions = plan.Conditions, StartsAt = plan.StartsAt };
        return response;
    }
}
public sealed class ReadPortalDashboardValidator : AbstractValidator<ReadPortalDashboardQuery> { public ReadPortalDashboardValidator() { RuleFor(item => item.CustomerExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); } }
public sealed class ReadPortalDashboardMediatorValidator : AbstractValidator<ReadPortalDashboardMediatorQuery> { public ReadPortalDashboardMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.Payload).NotNull().SetValidator(new ReadPortalDashboardValidator()).OverridePropertyName(string.Empty); } }
