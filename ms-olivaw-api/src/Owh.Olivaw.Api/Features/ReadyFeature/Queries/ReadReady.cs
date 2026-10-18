using FluentValidation;
using MediatR;
using Owh.OlivawCore.Api.Helpers;
using Owh.Olivaw.Dados;
using Owh.OlivawCore.Services;
using Owh.Olivaw.Api.Extensions.Https;

namespace Owh.Olivaw.Api.Features.ReadyFeature.Queries;

public class ReadReadyQuery
    : IRequest<ReadReadyQueryResponse>
{ }

public class ReadReadyQueryResponse
{
    public string Status { get; set; }

    public IList<ReadReadyDetailQueryResponse> Details { get; set; }
}

public class ReadReadyDetailQueryResponse
{
    public string Service { get; set; }
    public string Status { get; set; }
    public string ComponentType { get; set; }
    public string Address { get; set; }
    public string Output { get; set; }
    public string Time { get; set; }
}

public class ReadReadyQueryHandler
(
    WriteDbContext context,
    IConfiguration configuration
) : IRequestHandler<ReadReadyQuery,
    ReadReadyQueryResponse>
{
    private readonly WriteDbContext _context = context
        ?? throw new ArgumentNullException(nameof(context));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    public async Task<ReadReadyQueryResponse> Handle
    (
        ReadReadyQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var response = request.ToReadReadyQuery();

        var itens = new List<Task<ReadReadyDetailQueryResponse>>
        {
            GetPostgresDetailsAsync(response, cancellationToken)
        };

        await Task.WhenAll(itens);
        response.Details = itens.ToReadReadyDetailQueryResponse();

        return response;
    }

    private async Task<ReadReadyDetailQueryResponse>
        GetPostgresDetailsAsync
    (
        ReadReadyQueryResponse response,
        CancellationToken cancellationToken
    )
    {
        var canConnect = await _context.CanConnectAsync(cancellationToken);

        return response.ToReadReadyDetailPostgresSqlQueryResponse
        (
            canConnect,
            _configuration
        );
    }
}

public class ReadReadyQueryValidator
    : AbstractValidator<ReadReadyQuery>
{
    public ReadReadyQueryValidator() { }
}