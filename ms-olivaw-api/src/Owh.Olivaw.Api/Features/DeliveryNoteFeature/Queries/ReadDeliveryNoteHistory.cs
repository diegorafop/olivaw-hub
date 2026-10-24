using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Commands;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.DeliveryNoteFeature.Queries;
public sealed class ReadDeliveryNoteHistoryMediatorQuery : IRequest<ReadDeliveryNoteHistoryResponse> { public ReadDeliveryNoteHistoryQuery Payload { get; set; } = new(); }
public sealed class ReadDeliveryNoteHistoryHandler(IRepository<DeliveryNote> notes, IRepository<DeliveryNoteChange> changes) : IRequestHandler<ReadDeliveryNoteHistoryMediatorQuery, ReadDeliveryNoteHistoryResponse>
{
    public async Task<ReadDeliveryNoteHistoryResponse> Handle(ReadDeliveryNoteHistoryMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload; var note = await InventoryRules.RequireAsync(notes, p.ExternalId, token);
        var rows = (await changes.TakeOrderedAsync(item => item.DeliveryNoteId == note.Id && (p.BeforeId == 0 || item.Id < p.BeforeId), 21, item => item.Id, token)).ToList();
        var response = new ReadDeliveryNoteHistoryResponse { Items = rows.Take(20).Select(item => new DeliveryNoteHistoryItem { Revision = item.Revision, CreatedAt = item.CreatedAt, PublishedAt = item.PublishedAt, AuthorName = item.AuthorName, Title = item.Title, Summary = item.Summary, InternalNote = item.InternalNote, Version = item.Version, Category = item.Category, Status = item.Status, ApprovalReference = item.ApprovalReference, ServiceLabel = item.ServiceLabel, DeliveredAt = item.DeliveredAt }).ToList() }; if (rows.Count > 20) response.NextBeforeId = rows[19].Id; return response;
    }
}
public sealed class ReadDeliveryNoteHistoryValidator : AbstractValidator<ReadDeliveryNoteHistoryQuery> { public ReadDeliveryNoteHistoryValidator() { RuleFor(item => item.ExternalId).NotEmpty(); RuleFor(item => item.BeforeId).GreaterThanOrEqualTo(0); } }
public sealed class ReadDeliveryNoteHistoryMediatorValidator : AbstractValidator<ReadDeliveryNoteHistoryMediatorQuery> { public ReadDeliveryNoteHistoryMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadDeliveryNoteHistoryValidator()).OverridePropertyName(string.Empty); } }
