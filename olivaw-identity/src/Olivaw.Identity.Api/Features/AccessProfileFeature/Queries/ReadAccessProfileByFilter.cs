using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AccessProfileFeature.Queries;

public sealed class ReadAccessProfileByFilterQuery : IRequest<ReadAccessProfileByFilterResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
public sealed class ReadAccessProfileByFilterResponse
{
    public IReadOnlyList<AccessProfileResponse> Items { get; set; } = Array.Empty<AccessProfileResponse>();
}
public sealed class ReadAccessProfileByFilterValidator : AbstractValidator<ReadAccessProfileByFilterQuery>
{
    public ReadAccessProfileByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 1000000);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
    }
}
public sealed class ReadAccessProfileByFilterHandler(IdentityService service)
    : IRequestHandler<ReadAccessProfileByFilterQuery, ReadAccessProfileByFilterResponse>
{
    public async Task<ReadAccessProfileByFilterResponse> Handle(ReadAccessProfileByFilterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new ReadAccessProfileByFilterValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entities = await service.ListAsync<AccessProfile>(request.Page, request.PageSize, cancellationToken);
        var responses = new List<AccessProfileResponse>();
        foreach (var entity in entities)
            responses.Add(AccessProfileResponse.From(entity));
        return new ReadAccessProfileByFilterResponse { Items = responses };
    }
}
