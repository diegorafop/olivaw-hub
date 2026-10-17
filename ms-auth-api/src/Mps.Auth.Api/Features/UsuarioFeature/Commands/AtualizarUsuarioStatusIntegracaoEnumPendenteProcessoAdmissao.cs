using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand
    : EntityCommand,
      IRequest<IEnumerable<AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse>>
{
    public AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand() { }

    public AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse
{
    public Guid Id { get; set; }
}

public class AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandHandler
(
    IRepository<Usuario> repository,
    IServiceDocumento service
) : IRequestHandler<AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand,
    IEnumerable<AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse>>
{
    private readonly IRepository<Usuario> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IServiceDocumento _service = service
        ?? throw new ArgumentNullException(nameof(service));

    public async Task<IEnumerable<AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse>> Handle
    (
        AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var usuarioMany = await GetAsync(cancellationToken);

        var itens = await SelecionarUsuarioConcluiuProcessoAdmissaoManyAsync
        (
            usuarioMany,
            cancellationToken
        );

        await UpdateAsync(
            request,
            itens,
            cancellationToken
        );

        await _repository.SaveChangesAsync(cancellationToken);

        return itens
            .ToAtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandResponse();
    }

    private async Task<IEnumerable<Usuario>>
        SelecionarUsuarioConcluiuProcessoAdmissaoManyAsync
    (
        IEnumerable<Usuario> usuarioMany,
        CancellationToken cancellationToken
    )
    {
        var itens = await HttpRequestHelper.PerformBlockRequestAsync
        (
            usuarioMany,
            GetMaxRequestItemCount(),
            GetFuncAsync(cancellationToken)
        );

        return usuarioMany.IntersectBy
        (
            itens.Select(item => item.UsuarioId),
            usuario => usuario.IdExterno
        );
    }

    private Func<IEnumerable<Usuario>,
        Task<IEnumerable<SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQueryResponse>>>
        GetFuncAsync(CancellationToken cancellationToken)
    {
        return async item =>
        {
            var request = item.ToSelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQuery();

            return await _service.SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyAsync
            (
                request,
                cancellationToken
            );
        };
    }

    private static int GetMaxRequestItemCount()
    {
        return HttpRequestHelper.GetMaxRequestItemCount
        (
            "UsuarioIdMany",
            TextHelper.GetGuidLenght(),
            0.5f
        );
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
            .Equals(StatusIntegracaoEnum.PendenteProcessoAdmissao);
    }

    private async Task UpdateAsync
    (
        AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand request,
        IEnumerable<Usuario> itens,
        CancellationToken cancellationToken
    )
    {
        foreach (Usuario item in itens)
        {
            item.StatusIntegracaoEnum = StatusIntegracaoEnum.Pendente;
            item.Update(request.UsuarioLogadoId);
        }

        await _repository.UpdateCollectionAsync(itens, cancellationToken);
    }
}

public class AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandValidator :
    AbstractValidator<AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand>
{
    public AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommandValidator()
    {
        RuleFor(item => item.UsuarioLogadoId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand, Guid>
                (item => item.UsuarioLogadoId));
    }
}