using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AuthorizationFeature.Commands;

public sealed class CreateAuthorizationCommand : IRequest<CreateAuthorizationResponse>
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class CreateAuthorizationResponse
{
    public AuthorizationResponse Item { get; set; } = new();
}

public sealed class CreateAuthorizationHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<CreateAuthorizationCommand, CreateAuthorizationResponse>
{
    public async Task<CreateAuthorizationResponse> Handle(CreateAuthorizationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new CreateAuthorizationValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveAuthorizationAsync(null, request.Key, request.Name, request.Description, actor.Id, cancellationToken);
        return new CreateAuthorizationResponse { Item = AuthorizationResponse.From(entity) };
    }
}

public sealed class CreateAuthorizationValidator : AbstractValidator<CreateAuthorizationCommand>
{
    public CreateAuthorizationValidator()
    {
        RuleFor(item => item.Key).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Name).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Description).MaximumLength(500);
        RuleFor(item => item.Key).Matches(@"^[a-z0-9]+([._-][a-z0-9]+)*$");
    }
}
