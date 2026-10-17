using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Api.Security;

namespace Olivaw.Identity.Api.Features.AutenticacaoFeature.Commands;

public sealed class DeslogarUsuarioCommand : IRequest<DeslogarUsuarioResponse>
{

}
public sealed class DeslogarUsuarioResponse
{

}
public sealed class DeslogarUsuarioValidator : AbstractValidator<DeslogarUsuarioCommand>
{
    public DeslogarUsuarioValidator()
    {

    }
}
public sealed class DeslogarUsuarioHandler(AuthenticationService service, CurrentActor actor) : IRequestHandler<DeslogarUsuarioCommand, DeslogarUsuarioResponse>
{
    public async Task<DeslogarUsuarioResponse> Handle(DeslogarUsuarioCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new DeslogarUsuarioValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.LogoutAsync(actor.Id, cancellationToken);
        return new DeslogarUsuarioResponse();
    }
}
