using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature.Queries;

public sealed class ReadUserProfileAccessProfileByIdQuery : IRequest<ReadUserProfileAccessProfileByIdResponse>
{
    public Guid Id { get; set; }
}
public sealed class ReadUserProfileAccessProfileByIdResponse
{
    public UserProfileAccessProfileResponse Item { get; set; } = new();
}
public sealed class ReadUserProfileAccessProfileByIdValidator : AbstractValidator<ReadUserProfileAccessProfileByIdQuery>
{
    public ReadUserProfileAccessProfileByIdValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class ReadUserProfileAccessProfileByIdHandler(IdentityService service)
    : IRequestHandler<ReadUserProfileAccessProfileByIdQuery, ReadUserProfileAccessProfileByIdResponse>
{
    public async Task<ReadUserProfileAccessProfileByIdResponse> Handle(ReadUserProfileAccessProfileByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new ReadUserProfileAccessProfileByIdValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.FindAsync<UserProfileAccessProfile>(request.Id, cancellationToken);
        return new ReadUserProfileAccessProfileByIdResponse { Item = UserProfileAccessProfileResponse.From(entity) };
    }
}
