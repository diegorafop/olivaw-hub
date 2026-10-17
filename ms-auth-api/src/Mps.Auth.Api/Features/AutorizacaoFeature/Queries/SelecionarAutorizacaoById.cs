using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.AutorizacaoFeature.Queries;

public class SelecionarAutorizacaoByIdQuery
    : IRequest<SelecionarAutorizacaoByIdQueryResponse>
{
    public Guid Id { get; set; }
}

public class SelecionarAutorizacaoByIdQueryResponse
{
    public string Tipo { get; set; }
    public string Valor { get; set; }
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public bool Perfil { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class SelecionarAutorizacaoByIdQueryHandler
(
    IRepository<Autorizacao> repository
) : IRequestHandler<SelecionarAutorizacaoByIdQuery,
    SelecionarAutorizacaoByIdQueryResponse>
{
    public readonly IRepository<Autorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<SelecionarAutorizacaoByIdQueryResponse> Handle
    (
        SelecionarAutorizacaoByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Autorizacao entity = await GetFirstAsync
        (
            request,
            cancellationToken
        );

        return entity.Map<SelecionarAutorizacaoByIdQueryResponse>();
    }

    private async Task<Autorizacao> GetFirstAsync
    (
        SelecionarAutorizacaoByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.Id),
            cancellationToken
        );
    }
}

public class SelecionarAutorizacaoByIdQueryValidator
    : AbstractValidator<SelecionarAutorizacaoByIdQuery>
{
    public SelecionarAutorizacaoByIdQueryValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<SelecionarAutorizacaoByIdQuery, Guid>
                (item => item.Id));
    }
}