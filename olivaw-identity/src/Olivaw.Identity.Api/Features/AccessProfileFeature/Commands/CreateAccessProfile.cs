using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AccessProfileFeature.Commands;

public sealed class CreateAccessProfileCommand : IRequest<CreateAccessProfileResponse>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CreateAccessProfileResponse
{
    public AccessProfileResponse Item { get; set; } = new();
}

public sealed class CreateAccessProfileHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<CreateAccessProfileCommand, CreateAccessProfileResponse>
{
    public async Task<CreateAccessProfileResponse> Handle(CreateAccessProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new CreateAccessProfileValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveAccessProfileAsync(null, request.Name, request.Description, request.IsActive, actor.Id, cancellationToken);
        return new CreateAccessProfileResponse { Item = AccessProfileResponse.From(entity) };
    }
}

public sealed class CreateAccessProfileValidator : AbstractValidator<CreateAccessProfileCommand>
{
    public CreateAccessProfileValidator()
    {
        RuleFor(item => item.Name).NotEmpty().MaximumLength(100);
        RuleFor(item => item.Description).MaximumLength(500);
    }
}
