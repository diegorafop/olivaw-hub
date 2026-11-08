using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature.Commands;

public sealed class CreateUserProfileAccessProfileCommand : IRequest<CreateUserProfileAccessProfileResponse>
{
    public Guid UserProfileId { get; set; }
    public Guid AccessProfileId { get; set; }
}

public sealed class CreateUserProfileAccessProfileResponse
{
    public UserProfileAccessProfileResponse Item { get; set; } = new();
}

public sealed class CreateUserProfileAccessProfileHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<CreateUserProfileAccessProfileCommand, CreateUserProfileAccessProfileResponse>
{
    public async Task<CreateUserProfileAccessProfileResponse> Handle(CreateUserProfileAccessProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new CreateUserProfileAccessProfileValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveUserProfileAccessProfileAsync(null, request.UserProfileId, request.AccessProfileId, actor.Id, cancellationToken);
        return new CreateUserProfileAccessProfileResponse { Item = UserProfileAccessProfileResponse.From(entity) };
    }
}

public sealed class CreateUserProfileAccessProfileValidator : AbstractValidator<CreateUserProfileAccessProfileCommand>
{
    public CreateUserProfileAccessProfileValidator()
    {
        RuleFor(item => item.UserProfileId).NotEmpty();
        RuleFor(item => item.AccessProfileId).NotEmpty();
    }
}
