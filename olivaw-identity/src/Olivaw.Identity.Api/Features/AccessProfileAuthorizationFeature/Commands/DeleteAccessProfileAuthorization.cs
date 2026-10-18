using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature.Commands;

public sealed class DeleteAccessProfileAuthorizationCommand : IRequest<DeleteAccessProfileAuthorizationResponse>
{
    public Guid Id { get; set; }
}
public sealed class DeleteAccessProfileAuthorizationResponse { }
public sealed class DeleteAccessProfileAuthorizationValidator : AbstractValidator<DeleteAccessProfileAuthorizationCommand>
{
    public DeleteAccessProfileAuthorizationValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class DeleteAccessProfileAuthorizationHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<DeleteAccessProfileAuthorizationCommand, DeleteAccessProfileAuthorizationResponse>
{
    public async Task<DeleteAccessProfileAuthorizationResponse> Handle(DeleteAccessProfileAuthorizationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new DeleteAccessProfileAuthorizationValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.RemoveAsync<AccessProfileAuthorization>(request.Id, actor.Id, cancellationToken);
        return new DeleteAccessProfileAuthorizationResponse();
    }
}
