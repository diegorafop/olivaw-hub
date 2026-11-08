using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature.Commands;

public sealed class DeleteUserProfileAccessProfileCommand : IRequest<DeleteUserProfileAccessProfileResponse>
{
    public Guid Id { get; set; }
}
public sealed class DeleteUserProfileAccessProfileResponse { }
public sealed class DeleteUserProfileAccessProfileValidator : AbstractValidator<DeleteUserProfileAccessProfileCommand>
{
    public DeleteUserProfileAccessProfileValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class DeleteUserProfileAccessProfileHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<DeleteUserProfileAccessProfileCommand, DeleteUserProfileAccessProfileResponse>
{
    public async Task<DeleteUserProfileAccessProfileResponse> Handle(DeleteUserProfileAccessProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new DeleteUserProfileAccessProfileValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.RemoveAsync<UserProfileAccessProfile>(request.Id, actor.Id, cancellationToken);
        return new DeleteUserProfileAccessProfileResponse();
    }
}
