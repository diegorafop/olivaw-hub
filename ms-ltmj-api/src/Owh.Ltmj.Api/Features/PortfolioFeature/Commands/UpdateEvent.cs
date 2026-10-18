using FluentValidation;
using MediatR;
using Owh.Ltmj.Domain;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Api.Helpers;

namespace Owh.Ltmj.Api.Features.PortfolioFeature.Commands;

public sealed class UpdateEventCommand : IRequest<UpdateEventResponse>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    internal Guid UserId { get; set; }
}

public sealed class UpdateEventResponse {  }

public sealed class UpdateEventHandler(IRepository<PortfolioEvent> repository) : IRequestHandler<UpdateEventCommand, UpdateEventResponse>
{
    public async Task<UpdateEventResponse> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var item = await repository.GetFirstAsync(item => item.ExternalId == request.Id, cancellationToken);
        item.Update(request.Title, request.Description, request.Category, request.UserId);
        await repository.SaveChangesAsync(cancellationToken);
        return new UpdateEventResponse();
    }
}

public sealed class UpdateEventValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventValidator()
    {
        RuleFor(item => item.Id).NotEmpty();
        RuleFor(item => item.Title).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Description).NotEmpty().MaximumLength(4000);
        RuleFor(item => item.Category).Must(PortfolioEvent.IsCategory);
    }
}
