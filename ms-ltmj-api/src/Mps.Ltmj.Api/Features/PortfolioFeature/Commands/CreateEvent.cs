using FluentValidation;
using MediatR;
using Mps.Ltmj.Domain;
using Mps.OlivawCore.Repositories;
using Mps.OlivawCore.Api.Helpers;

namespace Mps.Ltmj.Api.Features.PortfolioFeature.Commands;

public sealed class CreateEventCommand : IRequest<CreateEventResponse>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    internal Guid UserId { get; set; }
}

public sealed class CreateEventResponse { public Guid Id { get; set; } }

public sealed class CreateEventHandler(IRepository<PortfolioEvent> repository) : IRequestHandler<CreateEventCommand, CreateEventResponse>
{
    public async Task<CreateEventResponse> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var item = PortfolioEvent.Create(request.Title, request.Description, request.Category, request.UserId);
        await repository.AddAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return new CreateEventResponse { Id = item.ExternalId };
    }
}

public sealed class CreateEventValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventValidator()
    {
        RuleFor(item => item.Title).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Description).NotEmpty().MaximumLength(4000);
        RuleFor(item => item.Category).Must(PortfolioEvent.IsCategory);
    }
}
