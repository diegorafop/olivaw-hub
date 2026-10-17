using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature.Commands;

public sealed class InserirUsuarioPerfilAcessoCommand : IRequest<InserirUsuarioPerfilAcessoResponse>
{
    public Guid UsuarioId { get; set; }
    public Guid PerfilAcessoId { get; set; }
}

public sealed class InserirUsuarioPerfilAcessoResponse
{
    public UsuarioPerfilAcessoResponse Registro { get; set; } = new();
}

public sealed class InserirUsuarioPerfilAcessoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<InserirUsuarioPerfilAcessoCommand, InserirUsuarioPerfilAcessoResponse>
{
    public async Task<InserirUsuarioPerfilAcessoResponse> Handle(InserirUsuarioPerfilAcessoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new InserirUsuarioPerfilAcessoValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveUsuarioPerfilAcessoAsync(null, request.UsuarioId, request.PerfilAcessoId, actor.Id, cancellationToken);
        return new InserirUsuarioPerfilAcessoResponse { Registro = UsuarioPerfilAcessoResponse.From(entity) };
    }
}

public sealed class InserirUsuarioPerfilAcessoValidator : AbstractValidator<InserirUsuarioPerfilAcessoCommand>
{
    public InserirUsuarioPerfilAcessoValidator()
    {
        RuleFor(item => item.UsuarioId).NotEmpty();
        RuleFor(item => item.PerfilAcessoId).NotEmpty();
    }
}
