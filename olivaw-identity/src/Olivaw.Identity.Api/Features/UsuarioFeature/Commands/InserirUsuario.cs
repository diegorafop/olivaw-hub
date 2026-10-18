using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UsuarioFeature.Commands;

public sealed class InserirUsuarioCommand : IRequest<InserirUsuarioResponse>
{
    public string Senha { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}

public sealed class InserirUsuarioResponse
{
    public UsuarioResponse Registro { get; set; } = new();
}

public sealed class InserirUsuarioHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<InserirUsuarioCommand, InserirUsuarioResponse>
{
    public async Task<InserirUsuarioResponse> Handle(InserirUsuarioCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new InserirUsuarioValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveUsuarioAsync(null, request.Login, request.Nome, request.Email, request.Senha, request.Ativo, actor.Id, cancellationToken);
        return new InserirUsuarioResponse { Registro = UsuarioResponse.From(entity) };
    }
}

public sealed class InserirUsuarioValidator : AbstractValidator<InserirUsuarioCommand>
{
    public InserirUsuarioValidator()
    {
        RuleFor(item => item.Login).NotEmpty().MaximumLength(100);
        RuleFor(item => item.Nome).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Email).NotEmpty().MaximumLength(254);
        RuleFor(item => item.Email).EmailAddress();
        RuleFor(item => item.Senha).NotEmpty().MaximumLength(128);
    }
}
