using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Commands;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.DeliveryNoteFeature.Commands;
public sealed class CreateDeliveryNoteMediatorCommand : IRequest<CreateDeliveryNoteResult> { public CreateDeliveryNoteCommand Payload { get; set; } = new(); public Guid ExternalId { get; set; } public Guid ActorUserId { get; set; } public string ActorName { get; set; } = string.Empty; }
public sealed class CreateDeliveryNoteResult { public string? Conflict { get; set; } public CreateDeliveryNoteResponse Response { get; set; } = new(); }
public sealed class CreateDeliveryNoteHandler(IRepository<DeliveryNote> notes, IRepository<Customer> customers, IRepository<ManagedService> services, IRepository<ManagedServiceCustomer> associations, IRepository<SupportRequest> requests) : IRequestHandler<CreateDeliveryNoteMediatorCommand, CreateDeliveryNoteResult>
{
    public async Task<CreateDeliveryNoteResult> Handle(CreateDeliveryNoteMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var data = new DeliveryNoteData { Title = p.Title, Summary = p.Summary, InternalNote = p.InternalNote, Version = p.Version, Category = p.Category, Status = p.Status, ApprovalReference = p.ApprovalReference, ServiceLabel = p.ServiceLabel, DeliveredAt = p.DeliveredAt };
        var customer = await InventoryRules.RequireAsync(customers, p.CustomerExternalId, token);
        ManagedService? service = null; SupportRequest? source = null;
        if (p.ServiceExternalId.HasValue)
        {
            service = await InventoryRules.RequireAsync(services, p.ServiceExternalId.Value, token);
            if (!await associations.ExistsAsync(item => item.CustomerId == customer.Id && item.ManagedServiceId == service.Id && item.EndedAt == null, token)) return new() { Conflict = "Serviço não vinculado à empresa." };
        }
        if (p.RequestExternalId.HasValue)
        {
            source = await InventoryRules.RequireAsync(requests, p.RequestExternalId.Value, token, item => item.ManagedService!);
            if (source.CustomerId != customer.Id) return new() { Conflict = "Pedido não pertence a esta empresa." };
            if (service != null && source.ManagedServiceId.HasValue && source.ManagedServiceId != service.Id) return new() { Conflict = "O serviço difere do pedido selecionado." };
            if (service == null) service = source.ManagedService;
        }
        var entity = DeliveryNote.Create(customer, service, source, data, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await notes.AddAsync(entity, token);
        if (!await InventoryRules.SaveAsync(notes, token)) return new() { Conflict = "Conflito na entrega. Reabra a ficha." };
        return new() { Response = new() { ExternalId = entity.ExternalId, Revision = entity.Revision } };
    }
}
public sealed class CreateDeliveryNoteValidator : AbstractValidator<CreateDeliveryNoteCommand>
{
    public CreateDeliveryNoteValidator()
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
        RuleFor(item => item.CustomerExternalId).NotEmpty(); RuleFor(item => item.ServiceExternalId).NotEqual(Guid.Empty); RuleFor(item => item.RequestExternalId).NotEqual(Guid.Empty); RuleFor(item => item.Status).Equal("draft");
    }
}
public sealed class CreateDeliveryNoteMediatorValidator : AbstractValidator<CreateDeliveryNoteMediatorCommand> { public CreateDeliveryNoteMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120); RuleFor(item => item.Payload).NotNull().SetValidator(new CreateDeliveryNoteValidator()).OverridePropertyName(string.Empty); } }
