using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Api.Security;

namespace Olivaw.Identity.Api.Features.AuthenticationFeature.Commands;

public sealed class LogoutUserCommand : IRequest<LogoutUserResponse>
{

}
public sealed class LogoutUserResponse
{

}
public sealed class LogoutUserValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserValidator()
    {

    }
}
public sealed class LogoutUserHandler(AuthenticationService service, CurrentActor actor) : IRequestHandler<LogoutUserCommand, LogoutUserResponse>
{
    public async Task<LogoutUserResponse> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new LogoutUserValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.LogoutAsync(actor.Id, cancellationToken);
        return new LogoutUserResponse();
    }
}
