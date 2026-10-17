using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Services;
using Olivaw.Identity.Api.Security;

namespace Olivaw.Identity.Api.Features.AutenticacaoFeature.Commands;

public sealed class AlterarSenhaCommand : IRequest<AlterarSenhaResponse>
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}
public sealed class AlterarSenhaResponse
{

}
public sealed class AlterarSenhaValidator : AbstractValidator<AlterarSenhaCommand>
{
    public AlterarSenhaValidator()
    {
        RuleFor(item => item.SenhaAtual).NotEmpty().MaximumLength(128);
        RuleFor(item => item.NovaSenha).NotEmpty().MinimumLength(12).MaximumLength(128);
    }
}
public sealed class AlterarSenhaHandler(AuthenticationService service, CurrentActor actor) : IRequestHandler<AlterarSenhaCommand, AlterarSenhaResponse>
{
    public async Task<AlterarSenhaResponse> Handle(AlterarSenhaCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new AlterarSenhaValidator().ValidateAndThrowAsync(request, cancellationToken);
        await service.ChangePasswordAsync(actor.Id, request.SenhaAtual, request.NovaSenha, cancellationToken);
        return new AlterarSenhaResponse();
    }
}
