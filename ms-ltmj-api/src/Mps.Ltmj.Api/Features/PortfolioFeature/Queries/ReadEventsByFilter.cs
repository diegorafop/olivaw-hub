using FluentValidation;
using MediatR;
using Mps.Ltmj.Domain;
using Mps.OlivawCore.Repositories;
using System.Linq.Expressions;
using Mps.OlivawCore.Api.Helpers;

namespace Mps.Ltmj.Api.Features.PortfolioFeature.Queries;

public sealed class ReadEventsByFilterQuery : IRequest<ReadEventsByFilterResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    internal bool PublicOnly { get; set; } = true;
}
public sealed class EventPhotoResponse
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
}
public sealed class EventResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public IReadOnlyList<EventPhotoResponse> Photos { get; set; } = [];
}
public sealed class ReadEventsByFilterResponse
{
    public IReadOnlyList<EventResponse> Items { get; set; } = [];
}
public sealed class ReadEventsByFilterHandler(IRepository<PortfolioEvent> repository)
    : IRequestHandler<ReadEventsByFilterQuery, ReadEventsByFilterResponse>
{
    public async Task<ReadEventsByFilterResponse> Handle(ReadEventsByFilterQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        Expression<Func<PortfolioEvent, bool>> filter = item => !request.PublicOnly || item.IsVisible;
        var items = await repository.GetAsync(filter, cancellationToken);
        var page = items
            .OrderByDescending(item => item.CreatedAt)
            .ThenBy(item => item.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);

        return new ReadEventsByFilterResponse { Items = page.Select(item => new EventResponse {
            Id = item.ExternalId, Title = item.Title, Description = item.Description, Category = item.Category,
            IsVisible = item.IsVisible,
            Photos = item.Photos.OrderBy(photo => photo.Position).Select(photo => new EventPhotoResponse {
                Id = photo.ExternalId, Url = $"portfolio/events/{item.ExternalId}/photos/{photo.ExternalId}"
            }).ToArray()
        }).ToArray() };
    }
}
public sealed class ReadEventsByFilterValidator : AbstractValidator<ReadEventsByFilterQuery>
{
    public ReadEventsByFilterValidator()
    {
        RuleFor(item => item.Page).InclusiveBetween(1, 10000);
        RuleFor(item => item.PageSize).InclusiveBetween(1, 50);
    }
}
