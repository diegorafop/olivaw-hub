using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AuthorizationFeature.Queries;

public sealed class ReadAuthorizationByFilterQuery : IRequest<ReadAuthorizationByFilterResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
public sealed class ReadAuthorizationByFilterResponse
{
    public IReadOnlyList<AuthorizationResponse> Items { get; set; } = Array.Empty<AuthorizationResponse>();
}
public sealed class ReadAuthorizationByFilterValidator : AbstractValidator<ReadAuthorizationByFilterQuery>
{
    public ReadAuthorizationByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 1000000);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
    }
}
public sealed class ReadAuthorizationByFilterHandler(IdentityService service)
    : IRequestHandler<ReadAuthorizationByFilterQuery, ReadAuthorizationByFilterResponse>
{
    public async Task<ReadAuthorizationByFilterResponse> Handle(ReadAuthorizationByFilterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new ReadAuthorizationByFilterValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entities = await service.ListAsync<Authorization>(request.Page, request.PageSize, cancellationToken);
        var responses = new List<AuthorizationResponse>();
        foreach (var entity in entities)
            responses.Add(AuthorizationResponse.From(entity));
        return new ReadAuthorizationByFilterResponse { Items = responses };
    }
}
