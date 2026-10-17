using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Hostings;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Requests;

namespace Mps.Auth.Api.Features.VersaoFeature.Queries;

public class SelecionarVersaoExternalQuery
    : SelecionarVersaoQuery,
      IRequest<SelecionarVersaoExternalQueryResponse>
{ }

public class SelecionarVersaoExternalQueryResponse
    : SelecionarVersaoQueryResponse
{ }

public class SelecionarVersaoExternalQueryHandler
(
    IConfiguration configuration
) : IRequestHandler<SelecionarVersaoExternalQuery,
    SelecionarVersaoExternalQueryResponse>
{
    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    public async Task<SelecionarVersaoExternalQueryResponse> Handle
    (
        SelecionarVersaoExternalQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        return await Task.FromResult
        (
            _configuration.GetSelecionarExternalVersaoQueryResponse()
        );
    }
}

public class SelecionarVersaoExternalQueryValidator
    : AbstractValidator<SelecionarVersaoExternalQuery>
{ }