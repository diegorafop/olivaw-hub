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
using System.Security.Cryptography;
namespace Owh.Olivaw.Api.Features.PortalFeature.Commands;
public sealed class CreatePortalInvitationMediatorCommand : IRequest<CreatePortalInvitationResult>
{
    public CreatePortalInvitationCommand Payload { get; set; } = new();
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
public sealed class CreatePortalInvitationResult { public string? Conflict { get; set; } public CreatePortalInvitationResponse Response { get; set; } = new(); }
public sealed class CreatePortalInvitationHandler(IRepository<Customer> customers, IRepository<PortalInvitation> invitations, [FromKeyedServices("portal")] IIdentityStore identity, IRepository<PortalAccessAudit> audits) : IRequestHandler<CreatePortalInvitationMediatorCommand, CreatePortalInvitationResult>
{
    public async Task<CreatePortalInvitationResult> Handle(CreatePortalInvitationMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); var p = request.Payload; var now = DateTime.UtcNow;
        var customer = await InventoryRules.RequireAsync(customers, p.CustomerExternalId, token);
        var normalized = p.Email.ToUpperInvariant();
        var user = await identity.FindAsync<UserProfile>(item => item.NormalizedLogin == normalized, token);
        if (user == null)
        {
            user = new UserProfile { ExternalId = Guid.NewGuid(), Login = p.Email, NormalizedLogin = normalized, Name = p.Name, Email = p.Email, IsActive = false, IsRoot = false, IsServiceAccount = false, ActorUserId = request.ActorUserId, CreatedAt = now };
            identity.Add(user); await identity.SaveAsync(token);
        }
        var pending = await invitations.TakeOrderedAsync(item => item.PortalUserExternalId == user.ExternalId && item.AcceptedAt == null && item.RevokedAt == null && item.ExpiresAt > now, 20, item => item.Id, token);
        if (pending.Count() >= 20) return new() { Conflict = "Há convites pendentes em excesso para este destinatário. Aguarde o vencimento ou revise os acessos." };
        var secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var invite = PortalInvitation.Create(user.ExternalId, customer, p.Email, p.Role, PortalAuthority.Hash(secret), request.ActorUserId, now, user.SecurityStamp);
        await invitations.AddAsync(invite, token);
        await audits.AddAsync(PortalAccessAudit.Create(user.ExternalId, customer, "invited", "Papel sugerido no convite: " + p.Role, "administration", request.ActorUserId, request.ActorName, now), token);
        await invitations.SaveChangesAsync(token);
        return new() { Response = new() { ExternalId = invite.ExternalId, Token = secret, ExpiresAt = invite.ExpiresAt } };
    }
}
public sealed class CreatePortalInvitationValidator : AbstractValidator<CreatePortalInvitationCommand>
{
    public CreatePortalInvitationValidator() { RuleFor(item => item.CustomerExternalId).NotEmpty(); RuleFor(item => item.Name).NotEmpty().MaximumLength(120); RuleFor(item => item.Email).NotEmpty().EmailAddress().MaximumLength(254); RuleFor(item => item.Role).Must(PortalMembership.IsRole); }
}
public sealed class CreatePortalInvitationMediatorValidator : AbstractValidator<CreatePortalInvitationMediatorCommand>
{
    public CreatePortalInvitationMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120); RuleFor(item => item.Payload).NotNull().SetValidator(new CreatePortalInvitationValidator()).OverridePropertyName(string.Empty); }
}
