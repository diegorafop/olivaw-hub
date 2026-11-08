using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AuthorizationFeature.Commands;

public sealed class DeleteAuthorizationCommand : IRequest<DeleteAuthorizationResponse>
{
    public Guid Id { get; set; }
}
public sealed class DeleteAuthorizationResponse { }
public sealed class DeleteAuthorizationValidator : AbstractValidator<DeleteAuthorizationCommand>
{
    public DeleteAuthorizationValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class DeleteAuthorizationHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<DeleteAuthorizationCommand, DeleteAuthorizationResponse>
{
    public async Task<DeleteAuthorizationResponse> Handle(DeleteAuthorizationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new DeleteAuthorizationValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.RemoveAsync<Authorization>(request.Id, actor.Id, cancellationToken);
        return new DeleteAuthorizationResponse();
    }
}
