using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature.Queries;

public sealed class ReadUserProfileAccessProfileByFilterQuery : IRequest<ReadUserProfileAccessProfileByFilterResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
public sealed class ReadUserProfileAccessProfileByFilterResponse
{
    public IReadOnlyList<UserProfileAccessProfileResponse> Items { get; set; } = Array.Empty<UserProfileAccessProfileResponse>();
}
public sealed class ReadUserProfileAccessProfileByFilterValidator : AbstractValidator<ReadUserProfileAccessProfileByFilterQuery>
{
    public ReadUserProfileAccessProfileByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 1000000);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 100);
    }
}
public sealed class ReadUserProfileAccessProfileByFilterHandler(IdentityService service)
    : IRequestHandler<ReadUserProfileAccessProfileByFilterQuery, ReadUserProfileAccessProfileByFilterResponse>
{
    public async Task<ReadUserProfileAccessProfileByFilterResponse> Handle(ReadUserProfileAccessProfileByFilterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new ReadUserProfileAccessProfileByFilterValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entities = await service.ListAsync<UserProfileAccessProfile>(request.Page, request.PageSize, cancellationToken);
        var responses = new List<UserProfileAccessProfileResponse>();
        foreach (var entity in entities)
            responses.Add(UserProfileAccessProfileResponse.From(entity));
        return new ReadUserProfileAccessProfileByFilterResponse { Items = responses };
    }
}
