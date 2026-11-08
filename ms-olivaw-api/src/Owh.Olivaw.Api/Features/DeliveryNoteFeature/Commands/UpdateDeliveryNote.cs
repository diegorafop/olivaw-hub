using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Commands;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.DeliveryNoteFeature.Commands;
public sealed class UpdateDeliveryNoteMediatorCommand : IRequest<UpdateDeliveryNoteResult> { public UpdateDeliveryNoteCommand Payload { get; set; } = new(); public Guid ExternalId { get; set; } public Guid ActorUserId { get; set; } public string ActorName { get; set; } = string.Empty; }
public sealed class UpdateDeliveryNoteResult { public string? Conflict { get; set; } public UpdateDeliveryNoteResponse Response { get; set; } = new(); }
public sealed class UpdateDeliveryNoteHandler(IRepository<DeliveryNote> notes, IRepository<DeliveryNoteChange> changes) : IRequestHandler<UpdateDeliveryNoteMediatorCommand, UpdateDeliveryNoteResult>
{
    public async Task<UpdateDeliveryNoteResult> Handle(UpdateDeliveryNoteMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var data = new DeliveryNoteData { Title = p.Title, Summary = p.Summary, InternalNote = p.InternalNote, Version = p.Version, Category = p.Category, Status = p.Status, ApprovalReference = p.ApprovalReference, ServiceLabel = p.ServiceLabel, DeliveredAt = p.DeliveredAt };
        var entity = await InventoryRules.RequireAsync(notes, request.ExternalId, token, item => item.SupportRequest!);
        if (entity.Revision != p.Revision) return new() { Conflict = "Esta entrega mudou. Reabra a ficha antes de salvar." };
        if (p.Status == "published" && entity.SupportRequest != null && entity.SupportRequest.Status is not ("delivered" or "closed")) return new() { Conflict = "Confirme a conclusão do pedido antes de publicar sua entrega." };
        var change = entity.Apply(data, request.ActorUserId, request.ActorName, DateTime.UtcNow); await changes.AddAsync(change, token); await notes.UpdateAsync(entity, token);
        if (!await InventoryRules.SaveAsync(notes, token)) return new() { Conflict = "Conflito na entrega. Reabra a ficha." };
        return new() { Response = new() { ExternalId = entity.ExternalId, Revision = entity.Revision } };
    }
}
public sealed class UpdateDeliveryNoteValidator : AbstractValidator<UpdateDeliveryNoteCommand>
{
    public UpdateDeliveryNoteValidator()
    {
        RuleFor(item => item.Title).NotNull().MaximumLength(120);
        RuleFor(item => item.Summary).NotNull().MaximumLength(3000);
        RuleFor(item => item.InternalNote).NotNull().MaximumLength(3000);
        RuleFor(item => item.Version).NotNull().MaximumLength(80);
        RuleFor(item => item.Category).NotNull().MaximumLength(20);
        RuleFor(item => item.Status).NotNull().MaximumLength(20);
        RuleFor(item => item.ApprovalReference).NotNull().MaximumLength(300);
        RuleFor(item => item.ServiceLabel).NotNull().MaximumLength(120);
        RuleFor(item => item.Title).NotEmpty(); RuleFor(item => item.Summary).NotEmpty();
        RuleFor(item => item.Status).Must(DeliveryNoteData.IsStatus); RuleFor(item => item.Category).Must(DeliveryNoteData.IsCategory);
        RuleFor(item => item.DeliveredAt).Must(value => !value.HasValue || value.Value.Kind == DateTimeKind.Utc);
        RuleFor(item => item.DeliveredAt).NotNull().LessThanOrEqualTo(_ => DateTime.UtcNow).When(item => item.Status == "published");
        RuleFor(item => item.ApprovalReference).NotEmpty().When(item => item.Status == "published");
        RuleFor(item => item.InternalNote).NotEmpty().When(item => item.Status == "withdrawn");
        RuleFor(item => item.Revision).GreaterThan(0);
    }
}
public sealed class UpdateDeliveryNoteMediatorValidator : AbstractValidator<UpdateDeliveryNoteMediatorCommand> { public UpdateDeliveryNoteMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120); RuleFor(item => item.ExternalId).NotEmpty(); RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateDeliveryNoteValidator()).OverridePropertyName(string.Empty); } }
