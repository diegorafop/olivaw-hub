using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Queries;

public class SelecionarUsuarioPerfilAcessoByIdQuery
    : IRequest<SelecionarUsuarioPerfilAcessoByIdQueryResponse>
{
    public Guid Id { get; set; }
}

public class SelecionarUsuarioPerfilAcessoByIdQueryResponse
{
    public Guid UsuarioId { get; set; }
    public Guid PerfilAcessoId { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class SelecionarUsuarioPerfilAcessoByIdQueryHandler
(
    IRepository<UsuarioPerfilAcesso> repository,
    SelecionarUsuarioPerfilAcessoByIdQueryValidator validator
) : IRequestHandler<SelecionarUsuarioPerfilAcessoByIdQuery,
    SelecionarUsuarioPerfilAcessoByIdQueryResponse>
{
    private readonly IRepository<UsuarioPerfilAcesso> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly SelecionarUsuarioPerfilAcessoByIdQueryValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<SelecionarUsuarioPerfilAcessoByIdQueryResponse> Handle
    (
        SelecionarUsuarioPerfilAcessoByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        UsuarioPerfilAcesso entity = await GetFirstAsync(request, cancellationToken);

        return entity.ToSelecionarUsuarioPerfilAcessoByIdQueryResponse();
    }

    private async Task<UsuarioPerfilAcesso> GetFirstAsync
    (
        SelecionarUsuarioPerfilAcessoByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var entity = await _repository.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.Id),
            cancellationToken,
            item => item.PerfilAcesso,
            item => item.Usuario
        );

        await _validator.ValidateDataAsync
        (
            request,
            entity,
            cancellationToken
        );

        return entity;
    }
}

public class SelecionarUsuarioPerfilAcessoByIdQueryValidator
    : AbstractValidator<SelecionarUsuarioPerfilAcessoByIdQuery>
{
    public SelecionarUsuarioPerfilAcessoByIdQueryValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<SelecionarUsuarioPerfilAcessoByIdQuery, Guid>
                (item => item.Id));
    }

    public async Task ValidateDataAsync
    (
        SelecionarUsuarioPerfilAcessoByIdQuery request,
        UsuarioPerfilAcesso entity,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.Id)
            .Must(item => { return entity != null; })
            .WithMessage(MessageHelper
                .NotFoundFor(request, item => item.Id));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}