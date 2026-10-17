using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.PerfilAcessoFeature.Queries;

public sealed class SelecionarPerfilAcessoByIdQuery : IRequest<SelecionarPerfilAcessoByIdResponse>
{
    public Guid Id { get; set; }
}
public sealed class SelecionarPerfilAcessoByIdResponse
{
    public PerfilAcessoResponse Registro { get; set; } = new();
}
public sealed class SelecionarPerfilAcessoByIdValidator : AbstractValidator<SelecionarPerfilAcessoByIdQuery>
{
    public SelecionarPerfilAcessoByIdValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class SelecionarPerfilAcessoByIdHandler(IdentityService service)
    : IRequestHandler<SelecionarPerfilAcessoByIdQuery, SelecionarPerfilAcessoByIdResponse>
{
    public async Task<SelecionarPerfilAcessoByIdResponse> Handle(SelecionarPerfilAcessoByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new SelecionarPerfilAcessoByIdValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.FindAsync<PerfilAcesso>(request.Id, cancellationToken);
        return new SelecionarPerfilAcessoByIdResponse { Registro = PerfilAcessoResponse.From(entity) };
    }
}
