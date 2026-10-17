using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AutorizacaoFeature.Commands;

public sealed class RemoverAutorizacaoCommand : IRequest<RemoverAutorizacaoResponse>
{
    public Guid Id { get; set; }
}
public sealed class RemoverAutorizacaoResponse { }
public sealed class RemoverAutorizacaoValidator : AbstractValidator<RemoverAutorizacaoCommand>
{
    public RemoverAutorizacaoValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class RemoverAutorizacaoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<RemoverAutorizacaoCommand, RemoverAutorizacaoResponse>
{
    public async Task<RemoverAutorizacaoResponse> Handle(RemoverAutorizacaoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new RemoverAutorizacaoValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.RemoveAsync<Autorizacao>(request.Id, actor.Id, cancellationToken);
        return new RemoverAutorizacaoResponse();
    }
}
