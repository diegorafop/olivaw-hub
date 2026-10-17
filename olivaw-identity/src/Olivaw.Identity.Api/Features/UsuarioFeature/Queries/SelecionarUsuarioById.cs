using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UsuarioFeature.Queries;

public sealed class SelecionarUsuarioByIdQuery : IRequest<SelecionarUsuarioByIdResponse>
{
    public Guid Id { get; set; }
}
public sealed class SelecionarUsuarioByIdResponse
{
    public UsuarioResponse Registro { get; set; } = new();
}
public sealed class SelecionarUsuarioByIdValidator : AbstractValidator<SelecionarUsuarioByIdQuery>
{
    public SelecionarUsuarioByIdValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class SelecionarUsuarioByIdHandler(IdentityService service)
    : IRequestHandler<SelecionarUsuarioByIdQuery, SelecionarUsuarioByIdResponse>
{
    public async Task<SelecionarUsuarioByIdResponse> Handle(SelecionarUsuarioByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new SelecionarUsuarioByIdValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.FindAsync<Usuario>(request.Id, cancellationToken);
        return new SelecionarUsuarioByIdResponse { Registro = UsuarioResponse.From(entity) };
    }
}
