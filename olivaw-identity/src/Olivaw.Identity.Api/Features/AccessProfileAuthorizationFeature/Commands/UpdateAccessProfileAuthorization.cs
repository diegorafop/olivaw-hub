using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature.Commands;

public sealed class UpdateAccessProfileAuthorizationCommand : IRequest<UpdateAccessProfileAuthorizationResponse>
{
    public Guid Id { get; set; }
    public Guid AccessProfileId { get; set; }
    public Guid AuthorizationId { get; set; }
}

public sealed class UpdateAccessProfileAuthorizationResponse
{
    public AccessProfileAuthorizationResponse Item { get; set; } = new();
}

public sealed class UpdateAccessProfileAuthorizationHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<UpdateAccessProfileAuthorizationCommand, UpdateAccessProfileAuthorizationResponse>
{
    public async Task<UpdateAccessProfileAuthorizationResponse> Handle(UpdateAccessProfileAuthorizationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new UpdateAccessProfileAuthorizationValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveAccessProfileAuthorizationAsync(request.Id, request.AccessProfileId, request.AuthorizationId, actor.Id, cancellationToken);
        return new UpdateAccessProfileAuthorizationResponse { Item = AccessProfileAuthorizationResponse.From(entity) };
    }
}

public sealed class UpdateAccessProfileAuthorizationValidator : AbstractValidator<UpdateAccessProfileAuthorizationCommand>
{
    public UpdateAccessProfileAuthorizationValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.AccessProfileId).NotEmpty();
        RuleFor(item => item.AuthorizationId).NotEmpty();
    }
}
