using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AutorizacaoFeature.Queries;

public sealed class SelecionarAutorizacaoByIdQuery : IRequest<SelecionarAutorizacaoByIdResponse>
{
    public Guid Id { get; set; }
}
public sealed class SelecionarAutorizacaoByIdResponse
{
    public AutorizacaoResponse Registro { get; set; } = new();
}
public sealed class SelecionarAutorizacaoByIdValidator : AbstractValidator<SelecionarAutorizacaoByIdQuery>
{
    public SelecionarAutorizacaoByIdValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class SelecionarAutorizacaoByIdHandler(IdentityService service)
    : IRequestHandler<SelecionarAutorizacaoByIdQuery, SelecionarAutorizacaoByIdResponse>
{
    public async Task<SelecionarAutorizacaoByIdResponse> Handle(SelecionarAutorizacaoByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new SelecionarAutorizacaoByIdValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.FindAsync<Autorizacao>(request.Id, cancellationToken);
        return new SelecionarAutorizacaoByIdResponse { Registro = AutorizacaoResponse.From(entity) };
    }
}
