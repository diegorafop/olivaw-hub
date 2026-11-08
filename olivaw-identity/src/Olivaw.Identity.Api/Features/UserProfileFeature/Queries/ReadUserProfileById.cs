using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UserProfileFeature.Queries;

public sealed class ReadUserProfileByIdQuery : IRequest<ReadUserProfileByIdResponse>
{
    public Guid Id { get; set; }
}
public sealed class ReadUserProfileByIdResponse
{
    public UserProfileResponse Item { get; set; } = new();
}
public sealed class ReadUserProfileByIdValidator : AbstractValidator<ReadUserProfileByIdQuery>
{
    public ReadUserProfileByIdValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class ReadUserProfileByIdHandler(IdentityService service)
    : IRequestHandler<ReadUserProfileByIdQuery, ReadUserProfileByIdResponse>
{
    public async Task<ReadUserProfileByIdResponse> Handle(ReadUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new ReadUserProfileByIdValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.FindAsync<UserProfile>(request.Id, cancellationToken);
        return new ReadUserProfileByIdResponse { Item = UserProfileResponse.From(entity) };
    }
}
