using FluentValidation;
using MediatR;
using Mps.OlivawCore.Api.Helpers;
using Mps.Limpo.Dados;
using Mps.OlivawCore.Services;
using Mps.Limpo.Api.Extensions.Https;

namespace Mps.Limpo.Api.Features.ReadyFeature.Queries;

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
    WriteDbContext context,
    IProviderAuthentication provider,
    IConfiguration configuration
) : IRequestHandler<SelecionarReadyQuery,
    SelecionarReadyQueryResponse>
{
    private readonly WriteDbContext _context = context
        ?? throw new ArgumentNullException(nameof(context));

    private readonly IProviderAuthentication _provider = provider
        ?? throw new ArgumentNullException(nameof(provider));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    public async Task<SelecionarReadyQueryResponse> Handle
    (
        SelecionarReadyQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var response = request.ToSelecionarReadyQuery();

        var itens = new List<Task<SelecionarReadyDetailQueryResponse>>
        {
            GetPostgresDetailsAsync(response, cancellationToken),
            GetProviderAuthenticationDetailsAsync(response, cancellationToken)
        };

        await Task.WhenAll(itens);
        response.Details = itens.ToSelecionarReadyDetailQueryResponse();

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
        GetPostgresDetailsAsync
    (
        SelecionarReadyQueryResponse response,
        CancellationToken cancellationToken
    )
    {
        var canConnect = await _context.CanConnectAsync(cancellationToken);

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