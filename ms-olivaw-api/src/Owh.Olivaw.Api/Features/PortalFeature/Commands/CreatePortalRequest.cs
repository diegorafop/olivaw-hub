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
using System.Text.Json;
using Owh.Olivaw.Api.Validation;
namespace Owh.Olivaw.Api.Features.PortalFeature.Commands;
public sealed class CreatePortalRequestMediatorCommand : IRequest<CreatePortalRequestResult>, IPreserveInputRequest { public CreatePortalRequestCommand Payload { get; set; } = new(); public Guid ActorUserId { get; set; } public string ActorName { get; set; } = string.Empty; }
public sealed class CreatePortalRequestResult { public string? Conflict { get; set; } public CreatePortalRequestResponse Response { get; set; } = new(); }
public sealed class CreatePortalRequestHandler(PortalAccess access, IRepository<SupportRequest> requests, IRepository<SupportMessage> messages, IRepository<ManagedService> services, IRepository<ManagedServiceCustomer> associations, IRepository<PortalServiceLink> publicLinks) : IRequestHandler<CreatePortalRequestMediatorCommand, CreatePortalRequestResult>
{
    public async Task<CreatePortalRequestResult> Handle(CreatePortalRequestMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload; var now = DateTime.UtcNow;
        var member = await access.RequireAsync(request.ActorUserId, p.CustomerExternalId, true, false, token);
        var digest = PortalAuthority.Hash(request.ActorUserId.ToString("N") + JsonSerializer.Serialize(p));
        var existing = await requests.GetFirstOrDefaultAsync(item => item.CustomerId == member.CustomerId && item.PortalSubmissionKey == p.RequestKey, token);
        if (existing != null)
        {
            if (existing.SubmissionDigest != digest) return new() { Conflict = "Esta chave já foi utilizada com outro pedido." };
            return new() { Response = new() { ExternalId = existing.ExternalId, Protocol = existing.Protocol } };
        }
        ManagedService? service = null;
        if (p.ServiceExternalId.HasValue)
        {
            service = await InventoryRules.RequireAsync(services, p.ServiceExternalId.Value, token);
            if (!await associations.ExistsAsync(item => item.CustomerId == member.CustomerId && item.ManagedServiceId == service.Id && item.EndedAt == null, token)) throw new IdentityFailure(403, "Serviço não disponível para esta empresa.");
            if (!await publicLinks.ExistsAsync(item => item.CustomerId == member.CustomerId && item.ManagedServiceId == service.Id && item.IsActive, token)) throw new IdentityFailure(403, "Serviço não disponível neste acesso.");
        }
        var category = "improvement"; if (p.RequestType == "question") category = "question"; if (p.RequestType == "help") category = "problem";
        var entity = SupportRequest.Create(member.Customer, new SupportRequestData { Title = p.Title.Trim(), Requester = request.ActorName, Impact = "Relatado pelo cliente; avaliação pendente.", Owner = "Equipe Olivaw", NextAction = "Avaliar solicitação recebida pelo portal", Category = category, Status = "received", PublicSummary = "Recebemos seu pedido. A equipe fará a avaliação." }, request.ActorUserId, request.ActorName, now, service);
        entity.RecordPortalSubmission(p.RequestKey, digest, p.RequestType, p.DesiredStorageGB, p.DesiredMemoryMiB, p.DesiredPlan);
        await requests.AddAsync(entity, token);
        await messages.AddAsync(SupportMessage.Create(entity, p.RequestKey, p.Description, true, false, request.ActorUserId, request.ActorName, now), token);
        if (!await InventoryRules.SaveAsync(requests, token)) return new() { Conflict = "Envio concorrente. Reenvie o mesmo pedido para conferir o protocolo." };
        return new() { Response = new() { ExternalId = entity.ExternalId, Protocol = entity.Protocol } };
    }
}
public sealed class CreatePortalRequestValidator : AbstractValidator<CreatePortalRequestCommand>
{
    public CreatePortalRequestValidator() { RuleFor(item => item.CustomerExternalId).NotEmpty(); RuleFor(item => item.ServiceExternalId).NotEqual(Guid.Empty); RuleFor(item => item.RequestKey).NotEmpty(); RuleFor(item => item.Title).NotEmpty().MaximumLength(120); RuleFor(item => item.Description).NotEmpty().MaximumLength(3000); RuleFor(item => item.RequestType).Must(value => value is "help" or "question" or "improvement" or "storage" or "capacity" or "plan"); RuleFor(item => item.DesiredStorageGB).GreaterThan(0).LessThanOrEqualTo(1000000000).PrecisionScale(18, 3, true); RuleFor(item => item.DesiredMemoryMiB).GreaterThan(0).LessThanOrEqualTo(1000000000).PrecisionScale(18, 3, true); RuleFor(item => item.DesiredPlan).NotNull().MaximumLength(120); }
}
public sealed class CreatePortalRequestMediatorValidator : AbstractValidator<CreatePortalRequestMediatorCommand> { public CreatePortalRequestMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120); RuleFor(item => item.Payload).NotNull().SetValidator(new CreatePortalRequestValidator()).OverridePropertyName(string.Empty); } }
