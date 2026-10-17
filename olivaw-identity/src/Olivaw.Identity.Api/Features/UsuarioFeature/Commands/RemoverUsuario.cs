using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UsuarioFeature.Commands;

public sealed class RemoverUsuarioCommand : IRequest<RemoverUsuarioResponse>
{
    public Guid Id { get; set; }
}
public sealed class RemoverUsuarioResponse { }
public sealed class RemoverUsuarioValidator : AbstractValidator<RemoverUsuarioCommand>
{
    public RemoverUsuarioValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class RemoverUsuarioHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<RemoverUsuarioCommand, RemoverUsuarioResponse>
{
    public async Task<RemoverUsuarioResponse> Handle(RemoverUsuarioCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new RemoverUsuarioValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.RemoveAsync<Usuario>(request.Id, actor.Id, cancellationToken);
        return new RemoverUsuarioResponse();
    }
}
