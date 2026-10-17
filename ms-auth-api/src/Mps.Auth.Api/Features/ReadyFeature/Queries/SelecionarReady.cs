using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions;
using Mps.Auth.Domain;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using Mps.HolosCore.Services;

namespace Mps.Auth.Api.Features.ReadyFeature.Queries;

public class SelecionarReadyQuery
    : IRequest<SelecionarReadyQueryResponse>
{ }

public class SelecionarReadyQueryResponse
{
    public string Status { get; set; }

    public IList<SelecionarReadyDetailQueryResponse> Details { get; set; }
}

public class SelecionarReadyDetailQueryResponse
{
    public string Service { get; set; }
    public string Status { get; set; }
    public string ComponentType { get; set; }
    public string Address { get; set; }
    public string Output { get; set; }
    public string Time { get; set; }
}

public class SelecionarReadyQueryHandler
(
    IRepository<Usuario> repository,
    IProviderAuthentication serviceAutenticacao,
    IConfiguration configuration,
    IServiceDocumento serviceDocumento,
    IServiceUsuario serviceUsuario,
    IServiceNotificacao serviceNotificacao
) : IRequestHandler<SelecionarReadyQuery,
    SelecionarReadyQueryResponse>
{
    private readonly IRepository<Usuario> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    private readonly IProviderAuthentication _provider = serviceAutenticacao
        ?? throw new ArgumentNullException(nameof(serviceAutenticacao));

    private readonly IServiceDocumento _serviceDocumento = serviceDocumento
        ?? throw new ArgumentNullException(nameof(serviceDocumento));

    private readonly IServiceUsuario _serviceUsuario = serviceUsuario
        ?? throw new ArgumentNullException(nameof(serviceUsuario));

    private readonly IServiceNotificacao _serviceNotificacao = serviceNotificacao
        ?? throw new ArgumentNullException(nameof(serviceNotificacao));

    public async Task<SelecionarReadyQueryResponse> Handle
    (
        SelecionarReadyQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var response = request.ToSelecionarReadyQueryResponse();

        var itens = new List<Task<SelecionarReadyDetailQueryResponse>>
        {
            GetPostgresDetailsAsync(response, cancellationToken),
            GetProviderAuthenticationDetailsAsync(response, cancellationToken),
            GetServiceDocumentoDetailsAsync(response, cancellationToken),
            GetServiceUsuarioDetailsAsync(response, cancellationToken),
            GetServiceNotificacaoDetailsAsync(response, cancellationToken)
        };

        await Task.WhenAll(itens);
        response.Details = itens.ToSelecionarReadyDetailQueryResponse();
        response.Status = response.GetHealthStatus();

        return response;
    }

    private async Task<SelecionarReadyDetailQueryResponse>
        GetProviderAuthenticationDetailsAsync
    (
        SelecionarReadyQueryResponse response,
        CancellationToken cancellationToken
    )
    {
        var healthy = await _provider.HealthAsync(cancellationToken);

        return response.ToSelecionarReadyDetailProviderAuthenticationQueryResponse
        (
            healthy,
            _provider
        );
    }

    private async Task<SelecionarReadyDetailQueryResponse>
        GetServiceDocumentoDetailsAsync
    (
        SelecionarReadyQueryResponse response,
        CancellationToken cancellationToken
    )
    {
        var healthy = await _serviceDocumento.HealthAsync(cancellationToken);

        return response.ToSelecionarReadyDetailServiceDocumentoQueryResponse
        (
            healthy,
            _serviceDocumento
        );
    }

    private async Task<SelecionarReadyDetailQueryResponse>
        GetServiceNotificacaoDetailsAsync
    (
        SelecionarReadyQueryResponse response,
        CancellationToken cancellationToken
    )
    {
        var healthy = await _serviceNotificacao.HealthAsync(cancellationToken);

        return response.ToSelecionarReadyDetailServiceNotificacaoQueryResponse
        (
            healthy,
            _serviceNotificacao
        );
    }

    private async Task<SelecionarReadyDetailQueryResponse>
        GetServiceUsuarioDetailsAsync
    (
        SelecionarReadyQueryResponse response,
        CancellationToken cancellationToken
    )
    {
        var healthy = await _serviceUsuario.HealthAsync(cancellationToken);

        return response.ToSelecionarReadyDetailServiceUsuarioQueryResponse
        (
            healthy,
            _serviceUsuario
        );
    }

    private async Task<SelecionarReadyDetailQueryResponse>
        GetPostgresDetailsAsync
    (
        SelecionarReadyQueryResponse response,
        CancellationToken cancellationToken
    )
    {
        var canConnect = await _repository.CanConnectAsync(cancellationToken);

        return response.ToSelecionarReadyDetailPostgresSqlQueryResponse
        (
            canConnect,
            _configuration
        );
    }
}

public class SelecionarReadyQueryValidator
    : AbstractValidator<SelecionarReadyQuery>
{

    public SelecionarReadyQueryValidator() { }
}