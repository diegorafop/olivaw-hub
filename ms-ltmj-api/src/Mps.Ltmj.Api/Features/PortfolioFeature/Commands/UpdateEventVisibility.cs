using FluentValidation;
using MediatR;
using Mps.Ltmj.Domain;
using Mps.OlivawCore.Repositories;
using Mps.OlivawCore.Api.Helpers;

namespace Mps.Ltmj.Api.Features.PortfolioFeature.Commands;

public sealed class UpdateEventVisibilityCommand : IRequest<UpdateEventVisibilityResponse>
{
    public Guid Id { get; set; }
    public bool IsVisible { get; set; }
    internal Guid UserId { get; set; }
}

public sealed class UpdateEventVisibilityResponse {  }

public sealed class UpdateEventVisibilityHandler(IRepository<PortfolioEvent> repository) : IRequestHandler<UpdateEventVisibilityCommand, UpdateEventVisibilityResponse>
{
    public async Task<UpdateEventVisibilityResponse> Handle(UpdateEventVisibilityCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var item = await repository.GetFirstAsync(item => item.ExternalId == request.Id, cancellationToken);
        item.SetVisibility(request.IsVisible, request.UserId);
        await repository.SaveChangesAsync(cancellationToken);
        return new UpdateEventVisibilityResponse();
    }
}

public sealed class UpdateEventVisibilityValidator : AbstractValidator<UpdateEventVisibilityCommand>
{
    public UpdateEventVisibilityValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
    }
}
