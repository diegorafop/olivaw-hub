using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Validation;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Commands;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Api.Features.SupportRequestFeature.Commands;
public sealed class CreateSupportMessageMediatorCommand : IRequest<CreateSupportMessageResult>, IPreserveInputRequest { public CreateSupportMessageCommand Payload { get; set; } = new(); public Guid ActorUserId { get; set; } public string ActorName { get; set; } = string.Empty; }
public sealed class CreateSupportMessageResult { public string? Conflict { get; set; } public CreateSupportMessageResponse Response { get; set; } = new(); }
public sealed class CreateSupportMessageHandler(IRepository<SupportRequest> requests, IRepository<SupportMessage> messages) : IRequestHandler<CreateSupportMessageMediatorCommand, CreateSupportMessageResult>
{
    public async Task<CreateSupportMessageResult> Handle(CreateSupportMessageMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload;
        var entity = await InventoryRules.RequireAsync(requests, p.RequestExternalId, token);
        var existing = await messages.GetFirstOrDefaultAsync(item => item.SupportRequestId == entity.Id && item.MessageKey == p.MessageKey, token);
        if (existing != null)
        {
            if (existing.Body != p.Body || existing.IsPublic != p.IsPublic || existing.ActorUserId != request.ActorUserId) return new() { Conflict = "Esta chave já foi utilizada com outra mensagem." };
            return new() { Response = new() { ExternalId = existing.ExternalId } };
        }
        var message = SupportMessage.Create(entity, p.MessageKey, p.Body, p.IsPublic, true, request.ActorUserId, request.ActorName, DateTime.UtcNow);
        await messages.AddAsync(message, token); if (!await InventoryRules.SaveAsync(messages, token)) return new() { Conflict = "Envio concorrente. Reenvie a mesma mensagem para conferir." };
        return new() { Response = new() { ExternalId = message.ExternalId } };
    }
}
public sealed class CreateSupportMessageValidator : AbstractValidator<CreateSupportMessageCommand> { public CreateSupportMessageValidator() { RuleFor(item => item.RequestExternalId).NotEmpty(); RuleFor(item => item.MessageKey).NotEmpty(); RuleFor(item => item.Body).NotEmpty().MaximumLength(3000); } }
public sealed class CreateSupportMessageMediatorValidator : AbstractValidator<CreateSupportMessageMediatorCommand> { public CreateSupportMessageMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120); RuleFor(item => item.Payload).NotNull().SetValidator(new CreateSupportMessageValidator()).OverridePropertyName(string.Empty); } }
