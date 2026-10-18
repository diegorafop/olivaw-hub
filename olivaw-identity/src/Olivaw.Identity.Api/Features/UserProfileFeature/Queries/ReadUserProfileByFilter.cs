using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UserProfileFeature.Queries;

public sealed class ReadUserProfileByFilterQuery : IRequest<ReadUserProfileByFilterResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
public sealed class ReadUserProfileByFilterResponse
{
    public IReadOnlyList<UserProfileResponse> Items { get; set; } = Array.Empty<UserProfileResponse>();
}
public sealed class ReadUserProfileByFilterValidator : AbstractValidator<ReadUserProfileByFilterQuery>
{
    public ReadUserProfileByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 1000000);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
    }
}
public sealed class ReadUserProfileByFilterHandler(IdentityService service)
    : IRequestHandler<ReadUserProfileByFilterQuery, ReadUserProfileByFilterResponse>
{
    public async Task<ReadUserProfileByFilterResponse> Handle(ReadUserProfileByFilterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new ReadUserProfileByFilterValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entities = await service.ListAsync<UserProfile>(request.Page, request.PageSize, cancellationToken);
        var responses = new List<UserProfileResponse>();
        foreach (var entity in entities)
            responses.Add(UserProfileResponse.From(entity));
        return new ReadUserProfileByFilterResponse { Items = responses };
    }
}
