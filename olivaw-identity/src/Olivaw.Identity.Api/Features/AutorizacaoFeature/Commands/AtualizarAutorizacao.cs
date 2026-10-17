using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AutorizacaoFeature.Commands;

public sealed class AtualizarAutorizacaoCommand : IRequest<AtualizarAutorizacaoResponse>
{
    public Guid Id { get; set; }
    public string Chave { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

public sealed class AtualizarAutorizacaoResponse
{
    public AutorizacaoResponse Registro { get; set; } = new();
}

public sealed class AtualizarAutorizacaoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<AtualizarAutorizacaoCommand, AtualizarAutorizacaoResponse>
{
    public async Task<AtualizarAutorizacaoResponse> Handle(AtualizarAutorizacaoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new AtualizarAutorizacaoValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveAutorizacaoAsync(request.Id, request.Chave, request.Nome, request.Descricao, actor.Id, cancellationToken);
        return new AtualizarAutorizacaoResponse { Registro = AutorizacaoResponse.From(entity) };
    }
}

public sealed class AtualizarAutorizacaoValidator : AbstractValidator<AtualizarAutorizacaoCommand>
{
    public AtualizarAutorizacaoValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.Chave).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Nome).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Descricao).MaximumLength(500);
        RuleFor(item => item.Chave).Matches(@"^[a-z0-9]+([._-][a-z0-9]+)*$");
    }
}
