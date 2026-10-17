using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature.Commands;

public sealed class AtualizarUsuarioPerfilAcessoCommand : IRequest<AtualizarUsuarioPerfilAcessoResponse>
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid PerfilAcessoId { get; set; }
}

public sealed class AtualizarUsuarioPerfilAcessoResponse
{
    public UsuarioPerfilAcessoResponse Registro { get; set; } = new();
}

public sealed class AtualizarUsuarioPerfilAcessoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<AtualizarUsuarioPerfilAcessoCommand, AtualizarUsuarioPerfilAcessoResponse>
{
    public async Task<AtualizarUsuarioPerfilAcessoResponse> Handle(AtualizarUsuarioPerfilAcessoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new AtualizarUsuarioPerfilAcessoValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveUsuarioPerfilAcessoAsync(request.Id, request.UsuarioId, request.PerfilAcessoId, actor.Id, cancellationToken);
        return new AtualizarUsuarioPerfilAcessoResponse { Registro = UsuarioPerfilAcessoResponse.From(entity) };
    }
}

public sealed class AtualizarUsuarioPerfilAcessoValidator : AbstractValidator<AtualizarUsuarioPerfilAcessoCommand>
{
    public AtualizarUsuarioPerfilAcessoValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.UsuarioId).NotEmpty();
        RuleFor(item => item.PerfilAcessoId).NotEmpty();
    }
}
