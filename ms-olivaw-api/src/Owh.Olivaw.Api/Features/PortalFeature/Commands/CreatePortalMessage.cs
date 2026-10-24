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
using Owh.Olivaw.Api.Validation;
namespace Owh.Olivaw.Api.Features.PortalFeature.Commands;
public sealed class CreatePortalMessageMediatorCommand : IRequest<CreatePortalMessageResult>, IPreserveInputRequest { public CreatePortalMessageCommand Payload { get; set; } = new(); public Guid ActorUserId { get; set; } public string ActorName { get; set; } = string.Empty; }
public sealed class CreatePortalMessageResult { public string? Conflict { get; set; } public CreatePortalMessageResponse Response { get; set; } = new(); }
public sealed class CreatePortalMessageHandler(PortalAccess access, IRepository<SupportRequest> requests, IRepository<SupportRequestChange> changes, IRepository<SupportMessage> messages) : IRequestHandler<CreatePortalMessageMediatorCommand, CreatePortalMessageResult>
{
    public async Task<CreatePortalMessageResult> Handle(CreatePortalMessageMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var member = await access.RequireAsync(request.ActorUserId, p.CustomerExternalId, true, false, token);
        var entity = await requests.GetFirstOrDefaultAsync(item => item.ExternalId == p.RequestExternalId && item.CustomerId == member.CustomerId && item.DeletedAt == null, token);
        if (entity == null) throw new IdentityFailure(403, "Solicitação não disponível para esta empresa.");
        var existing = await messages.GetFirstOrDefaultAsync(item => item.SupportRequestId == entity.Id && item.MessageKey == p.MessageKey, token);
        if (existing != null)
        {
            if (existing.Body != p.Body || existing.ActorUserId != request.ActorUserId || !existing.IsPublic) return new() { Conflict = "Esta chave já foi usada com outra mensagem." };
            return new() { Response = new() { ExternalId = existing.ExternalId } };
        }
        if (p.Reopen)
        {
            if (p.Revision != entity.Revision) return new() { Conflict = "Esta solicitação mudou. Atualize a conversa antes de pedir reabertura." };
            var change = entity.Apply(new() { Title = entity.Title, Requester = entity.Requester, Impact = entity.Impact, Owner = entity.Owner, NextAction = "Avaliar retorno do cliente", InternalNote = "Reabertura solicitada pelo cliente no portal.", PublicSummary = "Recebemos seu retorno e vamos reavaliar o pedido.", Category = entity.Category, Status = "analysis" }, request.ActorUserId, request.ActorName, DateTime.UtcNow);
            await changes.AddAsync(change, token); await requests.UpdateAsync(entity, token);
        }
        else if (entity.Status is "delivered" or "closed" or "cancelled") return new() { Conflict = "Solicite a reabertura para continuar este atendimento." };
        var message = SupportMessage.Create(entity, p.MessageKey, p.Body, true, false, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await messages.AddAsync(message, token); if (!await InventoryRules.SaveAsync(messages, token)) return new() { Conflict = "Envio concorrente. Confira a conversa e reenvie a mesma chave." };
        return new() { Response = new() { ExternalId = message.ExternalId } };
    }
}
public sealed class CreatePortalMessageValidator : AbstractValidator<CreatePortalMessageCommand> { public CreatePortalMessageValidator() { RuleFor(item => item.CustomerExternalId).NotEmpty(); RuleFor(item => item.RequestExternalId).NotEmpty(); RuleFor(item => item.MessageKey).NotEmpty(); RuleFor(item => item.Body).NotEmpty().MaximumLength(3000); RuleFor(item => item.Revision).GreaterThan(0).When(item => item.Reopen); } }
public sealed class CreatePortalMessageMediatorValidator : AbstractValidator<CreatePortalMessageMediatorCommand> { public CreatePortalMessageMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120); RuleFor(item => item.Payload).NotNull().SetValidator(new CreatePortalMessageValidator()).OverridePropertyName(string.Empty); } }
