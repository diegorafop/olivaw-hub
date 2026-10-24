using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature.Commands;

public sealed class UpdateUserProfileAccessProfileCommand : IRequest<UpdateUserProfileAccessProfileResponse>
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public Guid AccessProfileId { get; set; }
}

public sealed class UpdateUserProfileAccessProfileResponse
{
    public UserProfileAccessProfileResponse Item { get; set; } = new();
}

public sealed class UpdateUserProfileAccessProfileHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<UpdateUserProfileAccessProfileCommand, UpdateUserProfileAccessProfileResponse>
{
    public async Task<UpdateUserProfileAccessProfileResponse> Handle(UpdateUserProfileAccessProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new UpdateUserProfileAccessProfileValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveUserProfileAccessProfileAsync(request.Id, request.UserProfileId, request.AccessProfileId, actor.Id, cancellationToken);
        return new UpdateUserProfileAccessProfileResponse { Item = UserProfileAccessProfileResponse.From(entity) };
    }
}

public sealed class UpdateUserProfileAccessProfileValidator : AbstractValidator<UpdateUserProfileAccessProfileCommand>
{
    public UpdateUserProfileAccessProfileValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.UserProfileId).NotEmpty();
        RuleFor(item => item.AccessProfileId).NotEmpty();
    }
}
