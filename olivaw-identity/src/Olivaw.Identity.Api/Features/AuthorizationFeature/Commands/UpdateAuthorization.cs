using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AuthorizationFeature.Commands;

public sealed class UpdateAuthorizationCommand : IRequest<UpdateAuthorizationResponse>
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class UpdateAuthorizationResponse
{
    public AuthorizationResponse Item { get; set; } = new();
}

public sealed class UpdateAuthorizationHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<UpdateAuthorizationCommand, UpdateAuthorizationResponse>
{
    public async Task<UpdateAuthorizationResponse> Handle(UpdateAuthorizationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new UpdateAuthorizationValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveAuthorizationAsync(request.Id, request.Key, request.Name, request.Description, actor.Id, cancellationToken);
        return new UpdateAuthorizationResponse { Item = AuthorizationResponse.From(entity) };
    }
}

public sealed class UpdateAuthorizationValidator : AbstractValidator<UpdateAuthorizationCommand>
{
    public UpdateAuthorizationValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.Key).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Name).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Description).MaximumLength(500);
        RuleFor(item => item.Key).Matches(@"^[a-z0-9]+([._-][a-z0-9]+)*$");
    }
}
