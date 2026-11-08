using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UserProfileFeature.Commands;

public sealed class CreateUserProfileCommand : IRequest<CreateUserProfileResponse>
{
    public string Password { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsServiceAccount { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CreateUserProfileResponse
{
    public UserProfileResponse Item { get; set; } = new();
}

public sealed class CreateUserProfileHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<CreateUserProfileCommand, CreateUserProfileResponse>
{
    public async Task<CreateUserProfileResponse> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new CreateUserProfileValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveUserProfileAsync(null, request.Login, request.Name, request.Email, request.Password, request.IsActive, actor.Id, cancellationToken, request.IsServiceAccount);
        return new CreateUserProfileResponse { Item = UserProfileResponse.From(entity) };
    }
}

public sealed class CreateUserProfileValidator : AbstractValidator<CreateUserProfileCommand>
{
    public CreateUserProfileValidator()
    {
        RuleFor(item => item.Login).NotEmpty().MaximumLength(100);
        RuleFor(item => item.Name).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Email).NotEmpty().MaximumLength(254);
        RuleFor(item => item.Email).EmailAddress();
        RuleFor(item => item.Password).NotEmpty().MaximumLength(128);
    }
}
