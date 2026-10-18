using FluentValidation;
using MediatR;
using Mps.Ltmj.Domain;
using Mps.Ltmj.Services;
using Mps.OlivawCore.Repositories;
using Mps.OlivawCore.Api.Helpers;

namespace Mps.Ltmj.Api.Features.PortfolioFeature.Queries;

public sealed class ReadEventPhotoQuery : IRequest<ReadEventPhotoResponse>
{
    public Guid EventId { get; set; }
    public Guid PhotoId { get; set; }
    internal bool CanManage { get; set; }
}
public sealed class ReadEventPhotoResponse
{
    public Stream Content { get; set; } = Stream.Null;
    public string ContentType { get; set; } = string.Empty;
}
public sealed class ReadEventPhotoHandler(IRepository<PortfolioEvent> repository, IPhotoStorage storage)
    : IRequestHandler<ReadEventPhotoQuery, ReadEventPhotoResponse>
{
    public async Task<ReadEventPhotoResponse> Handle(ReadEventPhotoQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var item = await repository.GetFirstAsync(item => item.ExternalId == request.EventId &&
            (item.IsVisible || request.CanManage) && item.Photos.Any(photo => photo.ExternalId == request.PhotoId), cancellationToken);
        var photo = item.Photos.Single(photo => photo.ExternalId == request.PhotoId);
        return new ReadEventPhotoResponse { Content = await storage.OpenAsync(photo.StorageKey, cancellationToken), ContentType = photo.ContentType };
    }
}
public sealed class ReadEventPhotoValidator : AbstractValidator<ReadEventPhotoQuery>
{
    public ReadEventPhotoValidator()
    {
        RuleFor(item => item.EventId).NotEmpty();
        RuleFor(item => item.PhotoId).NotEmpty();
    }
}
