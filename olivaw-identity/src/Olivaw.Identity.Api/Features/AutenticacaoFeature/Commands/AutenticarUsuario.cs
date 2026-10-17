using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Api.Security;

namespace Olivaw.Identity.Api.Features.AutenticacaoFeature.Commands;

public sealed class AutenticarUsuarioCommand : IRequest<AutenticarUsuarioResponse>
{
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
public sealed class AutenticarUsuarioResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
public sealed class AutenticarUsuarioValidator : AbstractValidator<AutenticarUsuarioCommand>
{
    public AutenticarUsuarioValidator()
    {
        RuleFor(item => item.Login).NotEmpty().MaximumLength(100);
        RuleFor(item => item.Senha).NotEmpty().MaximumLength(128);
    }
}
public sealed class AutenticarUsuarioHandler(AuthenticationService service) : IRequestHandler<AutenticarUsuarioCommand, AutenticarUsuarioResponse>
{
    public async Task<AutenticarUsuarioResponse> Handle(AutenticarUsuarioCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new AutenticarUsuarioValidator().ValidateAndThrowAsync(request, cancellationToken);
        var result = await service.LoginAsync(request.Login, request.Senha, cancellationToken);
        return new AutenticarUsuarioResponse { AccessToken = result.AccessToken, ExpiresAt = result.ExpiresAt };
    }
}
