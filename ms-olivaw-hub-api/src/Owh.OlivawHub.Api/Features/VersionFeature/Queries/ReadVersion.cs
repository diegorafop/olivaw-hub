using FluentValidation;
using MassTransit;
using MediatR;
using Owh.OlivawCore.Api.Helpers;

namespace Owh.OlivawHub.Api.Features.VersionFeature.Queries;

public class ReadVersionQuery
    : IRequest<ReadVersionQueryResponse>
{ }

public class ReadVersionQueryResponse
{
    public string Version { get; set; }
    public string Title { get; set; }
}

public class ReadVersionQueryHandler
(
    IConfiguration configuration
) : IRequestHandler<ReadVersionQuery,
    ReadVersionQueryResponse>
{
    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    public async Task<ReadVersionQueryResponse> Handle
    (
        ReadVersionQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        return await Task.FromResult
        (
            new ReadVersionQueryResponse()
            {
                Version = GetVersion(),
                Title = GetTitle()
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

public class ReadVersionQueryValidator
    : AbstractValidator<ReadVersionQuery>
{ }