using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.PerfilAcessoFeature.Commands;

public sealed class InserirPerfilAcessoCommand : IRequest<InserirPerfilAcessoResponse>
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
}

public sealed class InserirPerfilAcessoResponse
{
    public PerfilAcessoResponse Registro { get; set; } = new();
}

public sealed class InserirPerfilAcessoHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<InserirPerfilAcessoCommand, InserirPerfilAcessoResponse>
{
    public async Task<InserirPerfilAcessoResponse> Handle(InserirPerfilAcessoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new InserirPerfilAcessoValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SavePerfilAcessoAsync(null, request.Nome, request.Descricao, request.Ativo, actor.Id, cancellationToken);
        return new InserirPerfilAcessoResponse { Registro = PerfilAcessoResponse.From(entity) };
    }
}

public sealed class InserirPerfilAcessoValidator : AbstractValidator<InserirPerfilAcessoCommand>
{
    public InserirPerfilAcessoValidator()
    {
        RuleFor(item => item.Nome).NotEmpty().MaximumLength(100);
        RuleFor(item => item.Descricao).MaximumLength(500);
    }
}
