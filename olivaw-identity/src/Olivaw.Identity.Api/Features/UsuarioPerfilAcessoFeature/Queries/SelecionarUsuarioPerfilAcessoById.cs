using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature.Queries;

public sealed class SelecionarUsuarioPerfilAcessoByIdQuery : IRequest<SelecionarUsuarioPerfilAcessoByIdResponse>
{
    public Guid Id { get; set; }
}
public sealed class SelecionarUsuarioPerfilAcessoByIdResponse
{
    public UsuarioPerfilAcessoResponse Registro { get; set; } = new();
}
public sealed class SelecionarUsuarioPerfilAcessoByIdValidator : AbstractValidator<SelecionarUsuarioPerfilAcessoByIdQuery>
{
    public SelecionarUsuarioPerfilAcessoByIdValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class SelecionarUsuarioPerfilAcessoByIdHandler(IdentityService service)
    : IRequestHandler<SelecionarUsuarioPerfilAcessoByIdQuery, SelecionarUsuarioPerfilAcessoByIdResponse>
{
    public async Task<SelecionarUsuarioPerfilAcessoByIdResponse> Handle(SelecionarUsuarioPerfilAcessoByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new SelecionarUsuarioPerfilAcessoByIdValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.FindAsync<UsuarioPerfilAcesso>(request.Id, cancellationToken);
        return new SelecionarUsuarioPerfilAcessoByIdResponse { Registro = UsuarioPerfilAcessoResponse.From(entity) };
    }
}
