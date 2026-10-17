using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.Auth.Repositories;
using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Helpers;

namespace Mps.Auth.Api.Features.UsuarioFeature.Queries;

public class SelecionarUsuarioByIdQuery :
      IRequest<SelecionarUsuarioByIdQueryResponse>
{
    public Guid Id { get; set; }
}

public class SelecionarUsuarioByIdQueryResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }

    public string Login { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Matricula { get; set; }
    public StatusIntegracaoEnum StatusIntegracaoEnum { get; set; }

    public Guid CargoId { get; set; }
    public string CargoNome { get; set; }
}

public class SelecionarUsuarioByIdQueryHandler(
    IRepositoryUsuario repository,
    IServiceUsuario service,
    IConfiguration configuration
    )
        : IRequestHandler<SelecionarUsuarioByIdQuery,
      SelecionarUsuarioByIdQueryResponse>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IServiceUsuario _service = service
        ?? throw new ArgumentNullException(nameof(service));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    public async Task<SelecionarUsuarioByIdQueryResponse> Handle
    (
        SelecionarUsuarioByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Usuario entity = await GetFirstAsync(request, cancellationToken);
        var usuarioCargo = await TrySelecionarUsuarioCargoByUsuarioIdAsync(entity, cancellationToken);

        return entity.ToSelecionarUsuarioByIdQueryResponse(usuarioCargo, _configuration);
    }

    private async Task<SelecionarUsuarioCargoByUsuarioIdQueryResponse>
        TrySelecionarUsuarioCargoByUsuarioIdAsync
    (
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        if (!_service.Configured)
            return entity.ToSelecionarUsuarioCargoByUsuarioIdQueryResponse();

        try
        {
            return await _service.SelecionarUsuarioCargoByUsuarioIdAsync
            (
                entity.IdExterno,
                cancellationToken
            );
        }
        catch
        {
            return entity.ToSelecionarUsuarioCargoByUsuarioIdQueryResponse();
        }
    }

    private async Task<Usuario> GetFirstAsync
    (
        SelecionarUsuarioByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.Id),
            cancellationToken,
            item => item.Autorizacoes
        );
    }
}

public class SelecionarUsuarioByIdQueryValidator
    : AbstractValidator<SelecionarUsuarioByIdQuery>
{
    public SelecionarUsuarioByIdQueryValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<SelecionarUsuarioByIdQuery, Guid>
                (item => item.Id));
    }
}