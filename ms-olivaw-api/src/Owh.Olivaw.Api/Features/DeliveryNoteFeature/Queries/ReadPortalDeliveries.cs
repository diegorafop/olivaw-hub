using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.DeliveryNoteFeature.Queries;
public sealed class ReadPortalDeliveriesMediatorQuery : IRequest<ReadPortalDeliveriesResponse> { public ReadPortalDeliveriesQuery Payload { get; set; } = new(); public Guid ActorUserId { get; set; } public bool Preview { get; set; } }
public sealed class ReadPortalDeliveriesHandler(PortalAccess access, IRepository<Customer> customers, IRepositoryDeliveryNote notes) : IRequestHandler<ReadPortalDeliveriesMediatorQuery, ReadPortalDeliveriesResponse>
{
    public async Task<ReadPortalDeliveriesResponse> Handle(ReadPortalDeliveriesMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload; Customer customer;
        if (request.Preview) customer = await InventoryRules.RequireAsync(customers, p.CustomerExternalId, token);
        else customer = (await access.RequireAsync(request.ActorUserId, p.CustomerExternalId, false, false, token)).Customer;
        var rows = await notes.ReadPublishedAsync(new() { CustomerId = customer.Id, ServiceExternalId = p.ServiceExternalId, Category = p.Category ?? "", FromAt = p.FromAt, ToAt = p.ToAt, BeforeAt = p.BeforeAt, BeforeId = p.BeforeId, Now = DateTime.UtcNow }, token);
        var response = new ReadPortalDeliveriesResponse { Items = rows.Take(20).Select(item => new PortalDeliveryItem { ExternalId = item.ExternalId, Title = item.Title, Summary = item.Summary, Version = item.Version, Category = item.Category, ServiceLabel = item.ServiceLabel, ServiceExternalId = item.ManagedService?.ExternalId, RequestExternalId = item.SupportRequest?.ExternalId, DeliveredAt = item.DeliveredAt!.Value, PublishedAt = item.PublishedAt!.Value, UpdatedAt = item.UpdatedAt, Revision = item.Revision }).ToList() };
        if (rows.Count > 20) { response.NextBeforeId = rows[19].Id; response.NextBeforeAt = rows[19].DeliveredAt; }
        return response;
    }
}
public sealed class ReadPortalDeliveriesValidator : AbstractValidator<ReadPortalDeliveriesQuery>
{
    public ReadPortalDeliveriesValidator()
    {
        RuleFor(item => item.CustomerExternalId).NotEmpty(); RuleFor(item => item.ServiceExternalId).NotEqual(Guid.Empty); RuleFor(item => item.Category).Must(value => value == null || value == "" || DeliveryNoteData.IsCategory(value));
        RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); RuleFor(item => item.BeforeAt).NotNull().When(item => item.BeforeId > 0); RuleFor(item => item.BeforeId).GreaterThan(0).When(item => item.BeforeAt.HasValue);
        RuleFor(item => item.FromAt).Must(value => !value.HasValue || value.Value.Kind == DateTimeKind.Utc); RuleFor(item => item.ToAt).Must(value => !value.HasValue || value.Value.Kind == DateTimeKind.Utc); RuleFor(item => item.BeforeAt).Must(value => !value.HasValue || value.Value.Kind == DateTimeKind.Utc);
        RuleFor(item => item.ToAt).GreaterThanOrEqualTo(item => item.FromAt).When(item => item.FromAt.HasValue && item.ToAt.HasValue);
    }
}
public sealed class ReadPortalDeliveriesMediatorValidator : AbstractValidator<ReadPortalDeliveriesMediatorQuery> { public ReadPortalDeliveriesMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.Payload).NotNull().SetValidator(new ReadPortalDeliveriesValidator()).OverridePropertyName(string.Empty); } }
