using FluentValidation;
using MediatR;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.OlivawCore.Api.Helpers;
namespace Owh.Olivaw.Api.Features.PortalFeature.Commands;
public sealed class DeletePortalSessionMediatorCommand : IRequest<DeletePortalSessionResponse> { public Guid ActorUserId { get; set; } }
public sealed class DeletePortalSessionHandler(PortalAuthentication authentication) : IRequestHandler<DeletePortalSessionMediatorCommand, DeletePortalSessionResponse>
{
    public async Task<DeletePortalSessionResponse> Handle(DeletePortalSessionMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request); await authentication.LogoutAsync(request.ActorUserId, token); return new() { SignedOut = true };
    }
}
public sealed class DeletePortalSessionMediatorValidator : AbstractValidator<DeletePortalSessionMediatorCommand> { public DeletePortalSessionMediatorValidator() { RuleFor(item => item.ActorUserId).NotEmpty(); } }
