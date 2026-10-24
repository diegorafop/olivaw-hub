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
namespace Owh.Olivaw.Api.Features.PortalFeature.Queries;
public sealed class ReadPortalInvitationMediatorQuery : IRequest<ReadPortalInvitationResponse> { public ReadPortalInvitationQuery Payload { get; set; } = new(); }
public sealed class ReadPortalInvitationHandler(IRepository<PortalInvitation> invitations, [FromKeyedServices("portal")] IIdentityStore identity) : IRequestHandler<ReadPortalInvitationMediatorQuery, ReadPortalInvitationResponse>
{
    public async Task<ReadPortalInvitationResponse> Handle(ReadPortalInvitationMediatorQuery request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var hash = PortalAuthority.Hash(request.Payload.Token);
        var invite = await invitations.GetFirstOrDefaultAsync(item => item.TokenHash == hash && item.DeletedAt == null, token, item => item.Customer);
        if (invite == null || !invite.CanAccept(invite.PortalUserExternalId, DateTime.UtcNow)) throw new IdentityFailure(400, "Convite indisponível.");
        var user = await identity.FindAsync<UserProfile>(item => item.ExternalId == invite.PortalUserExternalId, token);
        if (user == null) throw new IdentityFailure(400, "Convite indisponível.");
        if (!user.IsActive && user.SecurityStamp != invite.SecurityStampAtIssue) throw new IdentityFailure(400, "Convite indisponível.");
        return new() { Email = invite.Email, Business = invite.Customer.Business, ExistingAccount = user.IsActive, ExpiresAt = invite.ExpiresAt };
    }
}
public sealed class ReadPortalInvitationValidator : AbstractValidator<ReadPortalInvitationQuery> { public ReadPortalInvitationValidator() { RuleFor(item => item.Token).NotEmpty().Matches("^[A-Fa-f0-9]{64}$"); } }
public sealed class ReadPortalInvitationMediatorValidator : AbstractValidator<ReadPortalInvitationMediatorQuery> { public ReadPortalInvitationMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new ReadPortalInvitationValidator()).OverridePropertyName(string.Empty); } }
