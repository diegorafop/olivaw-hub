using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AccessProfileFeature.Queries;

public sealed class ReadAccessProfileByIdQuery : IRequest<ReadAccessProfileByIdResponse>
{
    public Guid Id { get; set; }
}
public sealed class ReadAccessProfileByIdResponse
{
    public AccessProfileResponse Item { get; set; } = new();
}
public sealed class ReadAccessProfileByIdValidator : AbstractValidator<ReadAccessProfileByIdQuery>
{
    public ReadAccessProfileByIdValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class ReadAccessProfileByIdHandler(IdentityService service)
    : IRequestHandler<ReadAccessProfileByIdQuery, ReadAccessProfileByIdResponse>
{
    public async Task<ReadAccessProfileByIdResponse> Handle(ReadAccessProfileByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new ReadAccessProfileByIdValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.FindAsync<AccessProfile>(request.Id, cancellationToken);
        return new ReadAccessProfileByIdResponse { Item = AccessProfileResponse.From(entity) };
    }
}
