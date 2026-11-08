using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature.Commands;

public sealed class CreateAccessProfileAuthorizationCommand : IRequest<CreateAccessProfileAuthorizationResponse>
{
    public Guid AccessProfileId { get; set; }
    public Guid AuthorizationId { get; set; }
}

public sealed class CreateAccessProfileAuthorizationResponse
{
    public AccessProfileAuthorizationResponse Item { get; set; } = new();
}

public sealed class CreateAccessProfileAuthorizationHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<CreateAccessProfileAuthorizationCommand, CreateAccessProfileAuthorizationResponse>
{
    public async Task<CreateAccessProfileAuthorizationResponse> Handle(CreateAccessProfileAuthorizationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new CreateAccessProfileAuthorizationValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveAccessProfileAuthorizationAsync(null, request.AccessProfileId, request.AuthorizationId, actor.Id, cancellationToken);
        return new CreateAccessProfileAuthorizationResponse { Item = AccessProfileAuthorizationResponse.From(entity) };
    }
}

public sealed class CreateAccessProfileAuthorizationValidator : AbstractValidator<CreateAccessProfileAuthorizationCommand>
{
    public CreateAccessProfileAuthorizationValidator()
    {
        RuleFor(item => item.AccessProfileId).NotEmpty();
        RuleFor(item => item.AuthorizationId).NotEmpty();
    }
}
