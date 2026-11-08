using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Commands;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.DeliveryNoteFeature.Queries;
public sealed class ReadDeliveryNoteByFilterMediatorQuery : IRequest<ReadDeliveryNoteByFilterResponse> { public ReadDeliveryNoteByFilterQuery Payload { get; set; } = new(); }
public sealed class ReadDeliveryNoteByFilterHandler(IRepository<DeliveryNote> notes) : IRequestHandler<ReadDeliveryNoteByFilterMediatorQuery, ReadDeliveryNoteByFilterResponse>
{
    public async Task<ReadDeliveryNoteByFilterResponse> Handle(ReadDeliveryNoteByFilterMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var rows = (await notes.TakeOrderedAsync(item => item.DeletedAt == null && (p.CustomerExternalId == null || item.Customer.ExternalId == p.CustomerExternalId) && (p.Status == null || item.Status == p.Status) && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token, item => item.Customer, item => item.ManagedService!, item => item.SupportRequest!)).ToList();
        var response = new ReadDeliveryNoteByFilterResponse { Items = rows.Take(20).Select(item => new DeliveryNoteItem { ExternalId = item.ExternalId, CustomerExternalId = item.Customer.ExternalId, ServiceExternalId = item.ManagedService?.ExternalId, RequestExternalId = item.SupportRequest?.ExternalId, Revision = item.Revision, CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt, PublishedAt = item.PublishedAt, Title = item.Title, Summary = item.Summary, InternalNote = item.InternalNote, Version = item.Version, Category = item.Category, Status = item.Status, ApprovalReference = item.ApprovalReference, ServiceLabel = item.ServiceLabel, DeliveredAt = item.DeliveredAt }).ToList() }; if (rows.Count > 20) response.NextBeforeId = rows[19].Id; return response;
    }
}
public sealed class ReadDeliveryNoteByFilterValidator : AbstractValidator<ReadDeliveryNoteByFilterQuery> { public ReadDeliveryNoteByFilterValidator() { RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); RuleFor(item => item.CustomerExternalId).NotEqual(Guid.Empty); RuleFor(item => item.Status).Must(value => value == null || DeliveryNoteData.IsStatus(value)); } }
public sealed class ReadDeliveryNoteByFilterMediatorValidator : AbstractValidator<ReadDeliveryNoteByFilterMediatorQuery> { public ReadDeliveryNoteByFilterMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadDeliveryNoteByFilterValidator()).OverridePropertyName(string.Empty); } }
