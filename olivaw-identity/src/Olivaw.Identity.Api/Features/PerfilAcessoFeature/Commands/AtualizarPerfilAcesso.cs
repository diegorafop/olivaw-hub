using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.PerfilAcessoFeature.Commands;

public sealed class AtualizarPerfilAcessoCommand : IRequest<AtualizarPerfilAcessoResponse>
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
}

public sealed class AtualizarPerfilAcessoResponse
{
    public PerfilAcessoResponse Registro { get; set; } = new();
}

public sealed class AtualizarPerfilAcessoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<AtualizarPerfilAcessoCommand, AtualizarPerfilAcessoResponse>
{
    public async Task<AtualizarPerfilAcessoResponse> Handle(AtualizarPerfilAcessoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new AtualizarPerfilAcessoValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SavePerfilAcessoAsync(request.Id, request.Nome, request.Descricao, request.Ativo, actor.Id, cancellationToken);
        return new AtualizarPerfilAcessoResponse { Registro = PerfilAcessoResponse.From(entity) };
    }
}

public sealed class AtualizarPerfilAcessoValidator : AbstractValidator<AtualizarPerfilAcessoCommand>
{
    public AtualizarPerfilAcessoValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.Nome).NotEmpty().MaximumLength(100);
        RuleFor(item => item.Descricao).MaximumLength(500);
    }
}
