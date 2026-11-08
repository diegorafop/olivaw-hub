using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Api.Security;

namespace Olivaw.Identity.Api.Features.AuthenticationFeature.Commands;

public sealed class ChangePasswordCommand : IRequest<ChangePasswordResponse>
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
public sealed class ChangePasswordResponse
{

}
public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(item => item.CurrentPassword).NotEmpty().MaximumLength(128);
        RuleFor(item => item.NewPassword).NotEmpty().MaximumLength(128);
    }
}
public sealed class ChangePasswordHandler(AuthenticationService service, CurrentActor actor) : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new ChangePasswordValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.ChangePasswordAsync(actor.Id, request.CurrentPassword, request.NewPassword, cancellationToken);
        return new ChangePasswordResponse();
    }
}
