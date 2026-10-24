using Owh.Olivaw.Api.Validation;
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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace Owh.Olivaw.Api.Features.PortalFeature.Commands;
public sealed class UpdatePortalInvitationMediatorCommand : IRequest<UpdatePortalInvitationResponse>, IPreserveInputRequest
{
    public UpdatePortalInvitationCommand Payload { get; set; } = new();
    public Guid AuthenticatedPortalUserId { get; set; }
}
public sealed class UpdatePortalInvitationHandler(IRepository<PortalInvitation> invitations, IRepository<PortalMembership> members, [FromKeyedServices("portal")] IIdentityStore identity, IPasswordHasher<UserProfile> hasher, IRepository<PortalAccessAudit> audits) : IRequestHandler<UpdatePortalInvitationMediatorCommand, UpdatePortalInvitationResponse>
{
    public async Task<UpdatePortalInvitationResponse> Handle(UpdatePortalInvitationMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var now = DateTime.UtcNow; var hash = PortalAuthority.Hash(request.Payload.Token);
        var invite = await invitations.GetFirstOrDefaultAsync(item => item.TokenHash == hash && item.DeletedAt == null, token, item => item.Customer);
        if (invite == null || !invite.CanAccept(invite.PortalUserExternalId, now)) throw new IdentityFailure(400, "Convite indisponível.");
        var user = await identity.FindAsync<UserProfile>(item => item.ExternalId == invite.PortalUserExternalId, token);
        if (user == null || user.IsRoot || user.IsServiceAccount) throw new IdentityFailure(400, "Convite indisponível.");
        if (user.IsActive)
        {
            if (request.AuthenticatedPortalUserId != user.ExternalId) throw new IdentityFailure(401, "Entre na conta destinatária para aceitar este convite.");
        }
        else
        {
            if (user.SecurityStamp != invite.SecurityStampAtIssue) throw new IdentityFailure(400, "Convite indisponível.");
            var password = request.Payload.Password;
            if (string.IsNullOrWhiteSpace(password) || password.Length < 12 || password.Length > 128) throw new IdentityFailure(400, "Use uma senha de 12 a 128 caracteres.");
            user.PasswordHash = hasher.HashPassword(user, password); user.IsActive = true; user.SecurityStamp = Guid.NewGuid(); user.ActorUserId = user.ExternalId; user.UpdatedAt = now;
            try { await identity.SaveAsync(token); }
            catch (DbUpdateConcurrencyException) { throw new IdentityFailure(409, "Este acesso mudou. Entre novamente para aceitar o convite."); }
        }
        var member = await members.GetFirstOrDefaultAsync(item => item.PortalUserExternalId == user.ExternalId && item.CustomerId == invite.CustomerId, token);
        if (member == null) await members.AddAsync(PortalMembership.Create(user.ExternalId, invite.Customer, invite.Role, invite.InvitedByUserId, "Convite aceito", now), token);
        else { member.Grant(invite.Role, invite.InvitedByUserId, "Convite aceito", now); await members.UpdateAsync(member, token); }
        await audits.AddAsync(PortalAccessAudit.Create(user.ExternalId, invite.Customer, "accepted", "Convite aceito; papel: " + invite.Role, "portal", user.ExternalId, user.Name, now), token);
        invite.Accept(user.ExternalId, now); await invitations.UpdateAsync(invite, token);
        if (!await InventoryRules.SaveAsync(invitations, token)) throw new IdentityFailure(409, "Este convite mudou. Entre novamente e confira os acessos.");
        return new() { Accepted = true };
    }
}
public sealed class UpdatePortalInvitationValidator : AbstractValidator<UpdatePortalInvitationCommand>
{
    public UpdatePortalInvitationValidator() { RuleFor(item => item.Token).NotEmpty().Matches("^[A-Fa-f0-9]{64}$"); RuleFor(item => item.Password).NotNull().MaximumLength(128); }
}
public sealed class UpdatePortalInvitationMediatorValidator : AbstractValidator<UpdatePortalInvitationMediatorCommand>
{
    public UpdatePortalInvitationMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new UpdatePortalInvitationValidator()).OverridePropertyName(string.Empty); }
}
