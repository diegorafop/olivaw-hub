using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.PerfilAcessoFeature.Commands;

public sealed class RemoverPerfilAcessoCommand : IRequest<RemoverPerfilAcessoResponse>
{
    public Guid Id { get; set; }
}
public sealed class RemoverPerfilAcessoResponse { }
public sealed class RemoverPerfilAcessoValidator : AbstractValidator<RemoverPerfilAcessoCommand>
{
    public RemoverPerfilAcessoValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class RemoverPerfilAcessoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<RemoverPerfilAcessoCommand, RemoverPerfilAcessoResponse>
{
    public async Task<RemoverPerfilAcessoResponse> Handle(RemoverPerfilAcessoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new RemoverPerfilAcessoValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.RemoveAsync<PerfilAcesso>(request.Id, actor.Id, cancellationToken);
        return new RemoverPerfilAcessoResponse();
    }
}
