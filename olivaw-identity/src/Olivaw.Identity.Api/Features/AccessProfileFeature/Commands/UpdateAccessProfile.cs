using FluentValidation;
using MediatR;
using Olivaw.Identity.Api.Security;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Features.AccessProfileFeature.Commands;

public sealed class UpdateAccessProfileCommand : IRequest<UpdateAccessProfileResponse>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateAccessProfileResponse
{
    public AccessProfileResponse Item { get; set; } = new();
}

public sealed class UpdateAccessProfileHandler(IdentityService service, CurrentActor actor)
    : IRequestHandler<UpdateAccessProfileCommand, UpdateAccessProfileResponse>
{
    public async Task<UpdateAccessProfileResponse> Handle(UpdateAccessProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await new UpdateAccessProfileValidator().ValidateAndThrowAsync(request, cancellationToken);
        var entity = await service.SaveAccessProfileAsync(request.Id, request.Name, request.Description, request.IsActive, actor.Id, cancellationToken);
        return new UpdateAccessProfileResponse { Item = AccessProfileResponse.From(entity) };
    }
}

public sealed class UpdateAccessProfileValidator : AbstractValidator<UpdateAccessProfileCommand>
{
    public UpdateAccessProfileValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.Name).NotEmpty().MaximumLength(100);
        RuleFor(item => item.Description).MaximumLength(500);
    }
}
