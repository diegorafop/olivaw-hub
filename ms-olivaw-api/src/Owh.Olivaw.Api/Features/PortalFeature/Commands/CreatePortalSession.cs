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
namespace Owh.Olivaw.Api.Features.PortalFeature.Commands;
public sealed class CreatePortalSessionMediatorCommand : IRequest<CreatePortalSessionResponse>, IPreserveInputRequest { public CreatePortalSessionCommand Payload { get; set; } = new(); }
public sealed class CreatePortalSessionHandler(PortalAuthentication authentication) : IRequestHandler<CreatePortalSessionMediatorCommand, CreatePortalSessionResponse>
{
    public async Task<CreatePortalSessionResponse> Handle(CreatePortalSessionMediatorCommand request, CancellationToken token)
    {
        ValidationHelper.ThrowIfNull(request);
        var result = await authentication.LoginAsync(request.Payload.Email, request.Payload.Password, token);
        return new() { AccessToken = result.AccessToken, ExpiresAt = result.ExpiresAt };
    }
}
public sealed class CreatePortalSessionValidator : AbstractValidator<CreatePortalSessionCommand>
{
    public CreatePortalSessionValidator() { RuleFor(item => item.Email).NotEmpty().EmailAddress().MaximumLength(254); RuleFor(item => item.Password).NotEmpty().MaximumLength(128); }
}
public sealed class CreatePortalSessionMediatorValidator : AbstractValidator<CreatePortalSessionMediatorCommand>
{
    public CreatePortalSessionMediatorValidator() { RuleFor(item => item.Payload).NotNull().SetValidator(new CreatePortalSessionValidator()).OverridePropertyName(string.Empty); }
}
