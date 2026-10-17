using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.Auth.Repositories;
using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Extensions.Collections;
using Mps.HolosCore.Api.Helpers;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioFeature.Queries;

public class SelecionarUsuarioByFilterQuery
    : IRequest<IEnumerable<SelecionarUsuarioByFilterQueryResponse>>
{
    public string? Login { get; set; }
    public bool? Ativo { get; set; }
    public IEnumerable<int>? Matriculas { get; set; }
    public IEnumerable<Guid>? Ids { get; set; }
    public StatusIntegracaoEnum? StatusIntegracaoEnum { get; set; }
}

public class SelecionarUsuarioByFilterQueryResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }

    public string Login { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public int Matricula { get; set; }
    public StatusIntegracaoEnum StatusIntegracaoEnum { get; set; }

    public Guid CargoId { get; set; }
    public string CargoNome { get; set; }
    public string IntegracaoId { get; set; }
}

public class SelecionarUsuarioByFilterQueryHandler
(
    IRepositoryUsuario repository,
    IServiceUsuario service,
    IConfiguration configuration
) : IRequestHandler<SelecionarUsuarioByFilterQuery,
    IEnumerable<SelecionarUsuarioByFilterQueryResponse>>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IServiceUsuario _service = service
        ?? throw new ArgumentNullException(nameof(service));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    public async Task<IEnumerable<SelecionarUsuarioByFilterQueryResponse>> Handle
    (
        SelecionarUsuarioByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        IEnumerable<Usuario> entities = await GetAsync(request, cancellationToken);

        var usuarioCargoMany = await SelecionarUsuarioCargoAsync(entities, cancellationToken);

        return entities.Select
        (
            item => item.ToSelecionarUsuarioByFilterQueryResponse
            (
                usuarioCargoMany.GetUsuarioCargoByUsuarioId(item.IdExterno),
                _configuration
            )
        );
    }

    private async Task<IEnumerable<SelecionarUsuarioCargoByFilterQueryResponse>>
        SelecionarUsuarioCargoAsync
    (
        IEnumerable<Usuario> entities,
        CancellationToken cancellationToken
    )
    {
        if (!_service.Configured)
            return entities.ToSelecionarUsuarioCargoByUsuarioIdQueryResponse();

        var maxRequestItemCount = HttpRequestHelper.GetMaxRequestItemCount
        (
            "UsuarioIds",
            TextHelper.GetGuidLenght(),
            0.5f
        );

        var usuarioIdMany = entities
            .Select(item => item.IdExterno)
            .ToList();

        return await HttpRequestHelper.PerformBlockRequestAsync
        (
            usuarioIdMany,
            maxRequestItemCount,
            async item =>
            {
                var request = new SelecionarUsuarioCargoByFilterQuery()
                {
                    UsuarioIds = item
                };

                return await _service.SelecionarUsuarioCargoByFilterAsync
                (
                    request,
                    cancellationToken
                );
            }
        );
    }

    private async Task<IEnumerable<Usuario>> GetAsync
    (
        SelecionarUsuarioByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetAsync
        (
            SearchPredicate(request),
            cancellationToken,
            item => item.Autorizacoes
        );
    }

    private static Expression<Func<Usuario, bool>> SearchPredicate
    (
        SelecionarUsuarioByFilterQuery request
    )
    {
        Expression<Func<Usuario, bool>> predicate = a => true;

        if (!string.IsNullOrEmpty(request.Login))
            predicate = predicate.And(item => item
                .Login
                .Contains(request.Login));

        if (request
            .Ativo
            .HasValue
        )
            predicate = SearchPredicateByAtivo(request, predicate);

        var itens = request
            .Ids
            ?.ToList();

        if (itens?.Any() ?? false)
            predicate = predicate.And(item => itens.Contains(item.IdExterno));

        var matriculaMany = request
            .Matriculas
            ?.Select(item => item.ToString())
            ?.ToList();

        if (matriculaMany?.Any() ?? false)
            predicate = SearchPredicateByMatricula(predicate, matriculaMany);

        if (request
            .StatusIntegracaoEnum
            .HasValue
        )
            predicate = predicate.And(item => item.StatusIntegracaoEnum == request.StatusIntegracaoEnum);

        return predicate;
    }

    private static Expression<Func<Usuario, bool>> SearchPredicateByMatricula
    (
        Expression<Func<Usuario, bool>> predicate,
        List<string> itens
    )
    {
        return predicate.And
        (
            item => (item.IntegracaoId != null && itens.Contains(item.IntegracaoId))
                || item
                    .Autorizacoes
                    .Any(item => item
                        .Autorizacao
                        .Tipo.Equals("matricula")
                        && itens
                            .Any(matricula => item
                                .Autorizacao
                                .Valor.Equals(matricula)))
        );
    }

    private static Expression<Func<Usuario, bool>> SearchPredicateByAtivo
    (
        SelecionarUsuarioByFilterQuery request,
        Expression<Func<Usuario, bool>> predicate
    )
    {
        if (!request
            .Ativo
            .HasValue
        )
            return predicate;

        if (request
            .Ativo
            .Value
        )
            predicate = predicate.And(item => item.DataRemocao == null);
        else
            predicate = predicate.And(item => item.DataRemocao != null);

        return predicate;
    }
}

public class SelecionarUsuarioByFilterQueryValidator
    : AbstractValidator<SelecionarUsuarioByFilterQuery>
{
    public SelecionarUsuarioByFilterQueryValidator() { }
}