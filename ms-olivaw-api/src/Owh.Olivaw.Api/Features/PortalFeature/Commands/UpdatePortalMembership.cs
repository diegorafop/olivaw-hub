using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Commands;
public sealed class UpdatePortalMembershipMediatorCommand : IRequest<UpdatePortalMembershipResult>
{
    public Guid ExternalId { get; set; } public Guid ActorUserId { get; set; } public string ActorName { get; set; } = string.Empty;
    public UpdatePortalMembershipCommand Payload { get; set; } = new();
}
public sealed class UpdatePortalMembershipResult { public string? Conflict { get; set; } public UpdatePortalMembershipResponse Response { get; set; } = new(); }
public sealed class UpdatePortalMembershipHandler(IRepository<PortalMembership> members, IRepository<PortalInvitation> invitations, IRepository<PortalAccessAudit> audits) : IRequestHandler<UpdatePortalMembershipMediatorCommand, UpdatePortalMembershipResult>
{
    public async Task<UpdatePortalMembershipResult> Handle(UpdatePortalMembershipMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var member = await InventoryRules.RequireAsync(members, request.ExternalId, token, item => item.Customer);
        if (member.Revision != request.Payload.Revision) return new() { Conflict = "Este acesso mudou. Reabra a lista." };
        var now = DateTime.UtcNow;
        var pending = await invitations.TakeOrderedAsync(item => item.PortalUserExternalId == member.PortalUserExternalId && item.CustomerId == member.CustomerId && item.AcceptedAt == null && item.RevokedAt == null && item.ExpiresAt > now, 20, item => item.Id, token);
        foreach (var invite in pending) invite.Revoke(request.ActorUserId, now);
        await invitations.UpdateCollectionAsync(pending, token); member.Revoke(request.ActorUserId, request.ActorName, now); await members.UpdateAsync(member, token);
        await audits.AddAsync(PortalAccessAudit.Create(member.PortalUserExternalId, member.Customer, "revoked", "Vínculo e convites pendentes desta empresa revogados.", "administration", request.ActorUserId, request.ActorName, now), token);
        if (!await InventoryRules.SaveAsync(members, token)) return new() { Conflict = "Este acesso mudou. Reabra a lista." };
        return new() { Response = new() { ExternalId = member.ExternalId, Revision = member.Revision } };
    }
}
public sealed class UpdatePortalMembershipValidator : AbstractValidator<UpdatePortalMembershipCommand> { public UpdatePortalMembershipValidator() { RuleFor(item => item.Revision).GreaterThan(0); } }
public sealed class UpdatePortalMembershipMediatorValidator : AbstractValidator<UpdatePortalMembershipMediatorCommand>
{
    public UpdatePortalMembershipMediatorValidator() { RuleFor(item => item.ExternalId).NotEmpty(); RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120); RuleFor(item => item.Payload).NotNull().SetValidator(new UpdatePortalMembershipValidator()).OverridePropertyName(string.Empty); }
}
