using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AutorizacaoFeature.Commands;

public sealed class InserirAutorizacaoCommand : IRequest<InserirAutorizacaoResponse>
{
    public string Chave { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

public sealed class InserirAutorizacaoResponse
{
    public AutorizacaoResponse Registro { get; set; } = new();
}

public sealed class InserirAutorizacaoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<InserirAutorizacaoCommand, InserirAutorizacaoResponse>
{
    public async Task<InserirAutorizacaoResponse> Handle(InserirAutorizacaoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new InserirAutorizacaoValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveAutorizacaoAsync(null, request.Chave, request.Nome, request.Descricao, actor.Id, cancellationToken);
        return new InserirAutorizacaoResponse { Registro = AutorizacaoResponse.From(entity) };
    }
}

public sealed class InserirAutorizacaoValidator : AbstractValidator<InserirAutorizacaoCommand>
{
    public InserirAutorizacaoValidator()
    {
        RuleFor(item => item.Chave).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Nome).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Descricao).MaximumLength(500);
        RuleFor(item => item.Chave).Matches(@"^[a-z0-9]+([._-][a-z0-9]+)*$");
    }
}
