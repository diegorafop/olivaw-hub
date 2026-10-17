using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class AtualizarUsuarioStatusIntegracaoEnumPendenteCommand
    : EntityCommand,
      IRequest<IEnumerable<AtualizarUsuarioStatusIntegracaoEnumPendenteCommandResponse>>
{
    public AtualizarUsuarioStatusIntegracaoEnumPendenteCommand() { }
    public AtualizarUsuarioStatusIntegracaoEnumPendenteCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AtualizarUsuarioStatusIntegracaoEnumPendenteCommandResponse
{
    public Guid Id { get; set; }
    public int Matricula { get; set; }
}

public class AtualizarUsuarioStatusIntegracaoEnumPendenteCommandHandler
(
    IRepository<Usuario> repository
) : IRequestHandler<AtualizarUsuarioStatusIntegracaoEnumPendenteCommand,
    IEnumerable<AtualizarUsuarioStatusIntegracaoEnumPendenteCommandResponse>>
{
    private readonly IRepository<Usuario> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<IEnumerable<AtualizarUsuarioStatusIntegracaoEnumPendenteCommandResponse>> Handle
    (
        AtualizarUsuarioStatusIntegracaoEnumPendenteCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var itens = await GetAsync(cancellationToken);

        await UpdateAsync(
            request,
            itens,
            cancellationToken
        );

        await _repository.SaveChangesAsync(cancellationToken);

        return itens.ToAtualizarUsuarioStatusIntegracaoPendenteCommandResponse();
    }

    private async Task<IEnumerable<Usuario>> GetAsync
    (
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetAsync
        (
            SearchPredicate(),
            cancellationToken
        );
    }

    private static Expression<Func<Usuario, bool>> SearchPredicate()
    {
        return item => item
            .StatusIntegracaoEnum
            .Equals(StatusIntegracaoEnum.Pendente);
    }

    private async Task UpdateAsync
    (
        AtualizarUsuarioStatusIntegracaoEnumPendenteCommand request,
        IEnumerable<Usuario> itens,
        CancellationToken cancellationToken
    )
    {
        foreach (Usuario item in itens)
        {
            item.StatusIntegracaoEnum = StatusIntegracaoEnum.Processando;
            item.Update(request.UsuarioLogadoId);
        }

        await _repository.UpdateCollectionAsync(itens, cancellationToken);
    }
}

public class AtualizarUsuarioStatusIntegracaoEnumPendenteCommandValidator :
    AbstractValidator<AtualizarUsuarioStatusIntegracaoEnumPendenteCommand>
{
    public AtualizarUsuarioStatusIntegracaoEnumPendenteCommandValidator()
    {
        RuleFor(item => item.UsuarioLogadoId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarUsuarioStatusIntegracaoEnumPendenteCommand, Guid>
                (item => item.UsuarioLogadoId));
    }
}