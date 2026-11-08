using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature.Queries;

public sealed class ReadAccessProfileAuthorizationByIdQuery : IRequest<ReadAccessProfileAuthorizationByIdResponse>
{
    public Guid Id { get; set; }
}
public sealed class ReadAccessProfileAuthorizationByIdResponse
{
    public AccessProfileAuthorizationResponse Item { get; set; } = new();
}
public sealed class ReadAccessProfileAuthorizationByIdValidator : AbstractValidator<ReadAccessProfileAuthorizationByIdQuery>
{
    public ReadAccessProfileAuthorizationByIdValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class ReadAccessProfileAuthorizationByIdHandler(IdentityService service)
    : IRequestHandler<ReadAccessProfileAuthorizationByIdQuery, ReadAccessProfileAuthorizationByIdResponse>
{
    public async Task<ReadAccessProfileAuthorizationByIdResponse> Handle(ReadAccessProfileAuthorizationByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new ReadAccessProfileAuthorizationByIdValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.FindAsync<AccessProfileAuthorization>(request.Id, cancellationToken);
        return new ReadAccessProfileAuthorizationByIdResponse { Item = AccessProfileAuthorizationResponse.From(entity) };
    }
}
