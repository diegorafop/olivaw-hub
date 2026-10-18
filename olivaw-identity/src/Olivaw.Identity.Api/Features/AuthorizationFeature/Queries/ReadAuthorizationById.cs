using FluentValidation;
using MediatR;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AuthorizationFeature.Queries;

public sealed class ReadAuthorizationByIdQuery : IRequest<ReadAuthorizationByIdResponse>
{
    public Guid Id { get; set; }
}
public sealed class ReadAuthorizationByIdResponse
{
    public AuthorizationResponse Item { get; set; } = new();
}
public sealed class ReadAuthorizationByIdValidator : AbstractValidator<ReadAuthorizationByIdQuery>
{
    public ReadAuthorizationByIdValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
public sealed class ReadAuthorizationByIdHandler(IdentityService service)
    : IRequestHandler<ReadAuthorizationByIdQuery, ReadAuthorizationByIdResponse>
{
    public async Task<ReadAuthorizationByIdResponse> Handle(ReadAuthorizationByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new ReadAuthorizationByIdValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.FindAsync<Authorization>(request.Id, cancellationToken);
        return new ReadAuthorizationByIdResponse { Item = AuthorizationResponse.From(entity) };
    }
}
