using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UsuarioFeature.Commands;

public sealed class AtualizarUsuarioCommand : IRequest<AtualizarUsuarioResponse>
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}

public sealed class AtualizarUsuarioResponse
{
    public UsuarioResponse Registro { get; set; } = new();
}

public sealed class AtualizarUsuarioHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<AtualizarUsuarioCommand, AtualizarUsuarioResponse>
{
    public async Task<AtualizarUsuarioResponse> Handle(AtualizarUsuarioCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new AtualizarUsuarioValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveUsuarioAsync(request.Id, request.Login, request.Nome, request.Email, null, request.Ativo, actor.Id, cancellationToken);
        return new AtualizarUsuarioResponse { Registro = UsuarioResponse.From(entity) };
    }
}

public sealed class AtualizarUsuarioValidator : AbstractValidator<AtualizarUsuarioCommand>
{
    public AtualizarUsuarioValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.Login).NotEmpty().MaximumLength(100);
        RuleFor(item => item.Nome).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Email).NotEmpty().MaximumLength(254);
        RuleFor(item => item.Email).EmailAddress();
    }
}
