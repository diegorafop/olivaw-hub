using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature.Commands;

public sealed class RemoverUsuarioPerfilAcessoCommand : IRequest<RemoverUsuarioPerfilAcessoResponse>
{
    public Guid Id { get; set; }
}
public sealed class RemoverUsuarioPerfilAcessoResponse { }
public sealed class RemoverUsuarioPerfilAcessoValidator : AbstractValidator<RemoverUsuarioPerfilAcessoCommand>
{
    public RemoverUsuarioPerfilAcessoValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class RemoverUsuarioPerfilAcessoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<RemoverUsuarioPerfilAcessoCommand, RemoverUsuarioPerfilAcessoResponse>
{
    public async Task<RemoverUsuarioPerfilAcessoResponse> Handle(RemoverUsuarioPerfilAcessoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new RemoverUsuarioPerfilAcessoValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.RemoveAsync<UsuarioPerfilAcesso>(request.Id, actor.Id, cancellationToken);
        return new RemoverUsuarioPerfilAcessoResponse();
    }
}
