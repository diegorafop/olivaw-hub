using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;
namespace Owh.Olivaw.Api.Features.PortalFeature.Commands;
public sealed class UpdatePortalRecoveryMediatorCommand : IRequest<UpdatePortalRecoveryResult> { public Guid ExternalId { get; set; } public Guid ActorUserId { get; set; } public string ActorName { get; set; } = string.Empty; public UpdatePortalRecoveryCommand Payload { get; set; } = new(); }
public sealed class UpdatePortalRecoveryResult { public string? Conflict { get; set; } public UpdatePortalRecoveryResponse Response { get; set; } = new(); }
public sealed class UpdatePortalRecoveryHandler(IRepository<PortalMembership> members, IRepository<PortalInvitation> invitations, IRepository<PortalAccessAudit> audits, [FromKeyedServices("portal")] IIdentityStore identity) : IRequestHandler<UpdatePortalRecoveryMediatorCommand, UpdatePortalRecoveryResult>
{
    public async Task<UpdatePortalRecoveryResult> Handle(UpdatePortalRecoveryMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var now = DateTime.UtcNow;
        var member = await InventoryRules.RequireAsync(members, request.ExternalId, token, item => item.Customer);
        if (!member.IsActive || member.Revision != request.Payload.Revision) return new() { Conflict = "Este vínculo mudou ou foi revogado. Confira os acessos." };
        var user = await identity.FindAsync<UserProfile>(item => item.ExternalId == member.PortalUserExternalId, token);
        if (user == null || user.IsRoot || user.IsServiceAccount) return new() { Conflict = "Conta não disponível para recuperação por este fluxo." };
        user.IsActive = false; user.PasswordHash = string.Empty; user.SecurityStamp = Guid.NewGuid(); user.LockedUntil = null; user.FailedLoginAttempts = 0; user.ActorUserId = request.ActorUserId; user.UpdatedAt = now;
        try { await identity.SaveAsync(token); } catch (DbUpdateConcurrencyException) { return new() { Conflict = "Este acesso mudou. Reabra a lista antes de recuperar." }; }
        var pending = await invitations.TakeOrderedAsync(item => item.PortalUserExternalId == user.ExternalId && item.AcceptedAt == null && item.RevokedAt == null && item.ExpiresAt > now, 20, item => item.Id, token);
        foreach (var invite in pending) invite.Revoke(request.ActorUserId, now);
        await invitations.UpdateCollectionAsync(pending, token);
        var secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var recovery = PortalInvitation.Create(user.ExternalId, member.Customer, user.Email, member.Role, PortalAuthority.Hash(secret), request.ActorUserId, now, user.SecurityStamp);
        await invitations.AddAsync(recovery, token);
        await audits.AddAsync(PortalAccessAudit.Create(user.ExternalId, member.Customer, "reset", request.Payload.Reason, "administration", request.ActorUserId, request.ActorName, now), token);
        if (!await InventoryRules.SaveAsync(invitations, token)) throw new IdentityFailure(409, "O acesso foi suspenso, mas o convite não foi confirmado. Gere um novo convite para o destinatário.");
        return new() { Response = new() { ExternalId = recovery.ExternalId, Token = secret, ExpiresAt = recovery.ExpiresAt } };
    }
}
public sealed class UpdatePortalRecoveryValidator : AbstractValidator<UpdatePortalRecoveryCommand> { public UpdatePortalRecoveryValidator() { RuleFor(item => item.Revision).GreaterThan(0); RuleFor(item => item.Reason).NotEmpty().MaximumLength(500); } }
public sealed class UpdatePortalRecoveryMediatorValidator : AbstractValidator<UpdatePortalRecoveryMediatorCommand> { public UpdatePortalRecoveryMediatorValidator() { RuleFor(item => item.ExternalId).NotEmpty(); RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120); RuleFor(item => item.Payload).NotNull().SetValidator(new UpdatePortalRecoveryValidator()).OverridePropertyName(string.Empty); } }
