using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Queries;

public class SelecionarUsuarioAutorizacaoByIdQuery
    : IRequest<SelecionarUsuarioAutorizacaoByIdQueryResponse>
{
    public Guid Id { get; set; }
}

public class SelecionarUsuarioAutorizacaoByIdQueryResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid AutorizacaoId { get; set; }
}

public class SelecionarUsuarioAutorizacaoByIdQueryHandler
(
    IRepository<UsuarioAutorizacao> repository
) : IRequestHandler<SelecionarUsuarioAutorizacaoByIdQuery,
    SelecionarUsuarioAutorizacaoByIdQueryResponse>
{
    public readonly IRepository<UsuarioAutorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<SelecionarUsuarioAutorizacaoByIdQueryResponse> Handle
    (
        SelecionarUsuarioAutorizacaoByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        UsuarioAutorizacao entity = await GetAsync
        (
            request,
            cancellationToken
        );

        return entity.ToSelecionarUsuarioAutorizacaoByIdQueryResponse();
    }

    private async Task<UsuarioAutorizacao> GetAsync
    (
        SelecionarUsuarioAutorizacaoByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            SearchPredicate(request),
            cancellationToken,
            item => item.Usuario,
            item => item.Autorizacao
        );
    }

    private static Expression<Func<UsuarioAutorizacao, bool>>
        SearchPredicate
    (
        SelecionarUsuarioAutorizacaoByIdQuery request
    )
    {
        return item => item
            .IdExterno
            .Equals(request.Id);
    }
}

public class SelecionarUsuarioAutorizacaoByIdQueryValidator
    : AbstractValidator<SelecionarUsuarioAutorizacaoByIdQuery>
{
    public SelecionarUsuarioAutorizacaoByIdQueryValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<SelecionarUsuarioAutorizacaoByIdQuery, Guid>
                (item => item.Id));
    }
}