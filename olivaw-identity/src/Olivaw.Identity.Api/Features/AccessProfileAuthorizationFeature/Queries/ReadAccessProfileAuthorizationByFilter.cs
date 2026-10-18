using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature.Queries;

public sealed class ReadAccessProfileAuthorizationByFilterQuery : IRequest<ReadAccessProfileAuthorizationByFilterResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
public sealed class ReadAccessProfileAuthorizationByFilterResponse
{
    public IReadOnlyList<AccessProfileAuthorizationResponse> Items { get; set; } = Array.Empty<AccessProfileAuthorizationResponse>();
}
public sealed class ReadAccessProfileAuthorizationByFilterValidator : AbstractValidator<ReadAccessProfileAuthorizationByFilterQuery>
{
    public ReadAccessProfileAuthorizationByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 1000000);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
    }
}
public sealed class ReadAccessProfileAuthorizationByFilterHandler(IdentityService service)
    : IRequestHandler<ReadAccessProfileAuthorizationByFilterQuery, ReadAccessProfileAuthorizationByFilterResponse>
{
    public async Task<ReadAccessProfileAuthorizationByFilterResponse> Handle(ReadAccessProfileAuthorizationByFilterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new ReadAccessProfileAuthorizationByFilterValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entities = await service.ListAsync<AccessProfileAuthorization>(request.Page, request.PageSize, cancellationToken);
        var responses = new List<AccessProfileAuthorizationResponse>();
        foreach (var entity in entities)
            responses.Add(AccessProfileAuthorizationResponse.From(entity));
        return new ReadAccessProfileAuthorizationByFilterResponse { Items = responses };
    }
}
