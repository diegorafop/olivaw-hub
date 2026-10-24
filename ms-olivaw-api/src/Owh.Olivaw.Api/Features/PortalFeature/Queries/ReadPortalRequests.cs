using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Services;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Queries;
public sealed class ReadPortalRequestsMediatorQuery : IRequest<ReadPortalRequestsResponse> { public ReadPortalRequestsQuery Payload { get; set; } = new(); public Guid ActorUserId { get; set; } public bool Preview { get; set; } }
public sealed class ReadPortalRequestsHandler(PortalAccess access, IRepository<Customer> customers, IRepository<SupportRequest> requests, IRepository<PortalServiceLink> links) : IRequestHandler<ReadPortalRequestsMediatorQuery, ReadPortalRequestsResponse>
{
    public async Task<ReadPortalRequestsResponse> Handle(ReadPortalRequestsMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        Customer customer;
        if (request.Preview) customer = await InventoryRules.RequireAsync(customers, p.CustomerExternalId, token);
        else customer = (await access.RequireAsync(request.ActorUserId, p.CustomerExternalId, false, false, token)).Customer;
        var rows = (await requests.TakeOrderedAsync(item => item.CustomerId == customer.Id && item.DeletedAt == null && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token, item => item.ManagedService!)).ToList();
        var serviceIds = rows.Take(20).Where(item => item.ManagedServiceId.HasValue).Select(item => item.ManagedServiceId!.Value).Distinct().ToArray();
        var names = (await links.GetAsync(item => item.CustomerId == customer.Id && serviceIds.Contains(item.ManagedServiceId) && item.IsActive && item.DeletedAt == null, token)).ToDictionary(item => item.ManagedServiceId, item => item.Label);
        var response = new ReadPortalRequestsResponse();
        foreach (var item in rows.Take(20))
        {
            var type = item.RequestType; if (type.Length == 0) type = item.Category;
            var label = ""; if (item.ManagedServiceId.HasValue) names.TryGetValue(item.ManagedServiceId.Value, out label);
            response.Items.Add(new() { ExternalId = item.ExternalId, Protocol = item.Protocol, Title = item.Title, Status = item.Status, RequestType = type, PublicSummary = item.PublicSummary, ServiceExternalId = item.ManagedService?.ExternalId, ServiceLabel = label ?? "", DesiredStorageGB = item.DesiredStorageGB, DesiredMemoryMiB = item.DesiredMemoryMiB, DesiredPlan = item.DesiredPlan, CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt, Revision = item.Revision });
        }
        if (rows.Count > 20) response.NextBeforeId = rows[19].Id; return response;
    }
}
public sealed class ReadPortalRequestsValidator : AbstractValidator<ReadPortalRequestsQuery> { public ReadPortalRequestsValidator() { RuleFor(item => item.CustomerExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); } }
public sealed class ReadPortalRequestsMediatorValidator : AbstractValidator<ReadPortalRequestsMediatorQuery> { public ReadPortalRequestsMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.Payload).NotNull().SetValidator(new ReadPortalRequestsValidator()).OverridePropertyName(string.Empty); } }
