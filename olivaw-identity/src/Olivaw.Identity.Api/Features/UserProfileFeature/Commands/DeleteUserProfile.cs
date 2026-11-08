using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UserProfileFeature.Commands;

public sealed class DeleteUserProfileCommand : IRequest<DeleteUserProfileResponse>
{
    public Guid Id { get; set; }
}
public sealed class DeleteUserProfileResponse { }
public sealed class DeleteUserProfileValidator : AbstractValidator<DeleteUserProfileCommand>
{
    public DeleteUserProfileValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class DeleteUserProfileHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<DeleteUserProfileCommand, DeleteUserProfileResponse>
{
    public async Task<DeleteUserProfileResponse> Handle(DeleteUserProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new DeleteUserProfileValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.RemoveAsync<UserProfile>(request.Id, actor.Id, cancellationToken);
        return new DeleteUserProfileResponse();
    }
}
