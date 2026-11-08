using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UserProfileFeature.Commands;

public sealed class UpdateUserProfileCommand : IRequest<UpdateUserProfileResponse>
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsServiceAccount { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateUserProfileResponse
{
    public UserProfileResponse Item { get; set; } = new();
}

public sealed class UpdateUserProfileHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileResponse>
{
    public async Task<UpdateUserProfileResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new UpdateUserProfileValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveUserProfileAsync(request.Id, request.Login, request.Name, request.Email, null, request.IsActive, actor.Id, cancellationToken, request.IsServiceAccount);
        return new UpdateUserProfileResponse { Item = UserProfileResponse.From(entity) };
    }
}

public sealed class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.Login).NotEmpty().MaximumLength(100);
        RuleFor(item => item.Name).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Email).NotEmpty().MaximumLength(254);
        RuleFor(item => item.Email).EmailAddress();
    }
}
