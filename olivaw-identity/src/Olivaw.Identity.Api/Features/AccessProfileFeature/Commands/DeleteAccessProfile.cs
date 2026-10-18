using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AccessProfileFeature.Commands;

public sealed class DeleteAccessProfileCommand : IRequest<DeleteAccessProfileResponse>
{
    public Guid Id { get; set; }
}
public sealed class DeleteAccessProfileResponse { }
public sealed class DeleteAccessProfileValidator : AbstractValidator<DeleteAccessProfileCommand>
{
    public DeleteAccessProfileValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class DeleteAccessProfileHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<DeleteAccessProfileCommand, DeleteAccessProfileResponse>
{
    public async Task<DeleteAccessProfileResponse> Handle(DeleteAccessProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new DeleteAccessProfileValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.RemoveAsync<AccessProfile>(request.Id, actor.Id, cancellationToken);
        return new DeleteAccessProfileResponse();
    }
}
