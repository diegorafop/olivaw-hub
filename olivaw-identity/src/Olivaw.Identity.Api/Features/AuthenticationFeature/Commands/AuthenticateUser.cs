using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Api.Security;

namespace Olivaw.Identity.Api.Features.AuthenticationFeature.Commands;

public sealed class AuthenticateUserCommand : IRequest<AuthenticateUserResponse>
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public sealed class AuthenticateUserResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
public sealed class AuthenticateUserValidator : AbstractValidator<AuthenticateUserCommand>
{
    public AuthenticateUserValidator()
    {
        RuleFor(item => item.Login).NotEmpty().MaximumLength(100);
        RuleFor(item => item.Password).NotEmpty().MaximumLength(128);
    }
}
public sealed class AuthenticateUserHandler(AuthenticationService service) : IRequestHandler<AuthenticateUserCommand, AuthenticateUserResponse>
{
    public async Task<AuthenticateUserResponse> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new AuthenticateUserValidator().ValidateAndThrowAsync(request, cancellationToken);
        var result = await service.LoginAsync(request.Login, request.Password, cancellationToken);
        return new AuthenticateUserResponse { AccessToken = result.AccessToken, ExpiresAt = result.ExpiresAt };
    }
}
