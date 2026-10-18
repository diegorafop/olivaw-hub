using FluentValidation;
using MassTransit;
using MediatR;
using Mps.OlivawCore.Api.Helpers;

namespace Mps.Ltmj.Api.Features.VersaoFeature.Queries;

public class SelecionarVersaoQuery
    : IRequest<SelecionarVersaoQueryResponse>
{ }

public class SelecionarVersaoQueryResponse
{
    public string Versao { get; set; }
    public string Titulo { get; set; }
}

public class SelecionarVersaoQueryHandler
(
    IConfiguration configuration
) : IRequestHandler<SelecionarVersaoQuery,
    SelecionarVersaoQueryResponse>
{
    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    public async Task<SelecionarVersaoQueryResponse> Handle
    (
        SelecionarVersaoQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        return await Task.FromResult
        (
            new SelecionarVersaoQueryResponse()
            {
                Versao = GetVersion(),
                Titulo = GetTitle()
            }
        );
    }

    private string GetTitle()
    {
        var title = _configuration.GetValue<string>("swagger:title");

        if (string.IsNullOrEmpty(title))
            return string.Empty;

        return title;
    }

    private string GetVersion()
    {
        var version = _configuration.GetValue<string>("swagger:version");

        if (string.IsNullOrEmpty(version))
            return string.Empty;

        return version;
    }
}

public class SelecionarVersaoQueryValidator
    : AbstractValidator<SelecionarVersaoQuery>
{ }