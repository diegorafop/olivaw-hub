using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Mps.Ltmj.Domain;
using Mps.Ltmj.Portfolio;
using Mps.OlivawCore.Repositories;
using Mps.Ltmj.Tests.Mocks;
using Mps.Ltmj.Api.Features.PortfolioFeature.Queries;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest.Queries;

public class ReadEventPhotoHandlerTest
{
    [Fact]
    public async Task Handle_should_restrict_hidden_photos_and_stream_content()
    {
        var item = PortfolioEventFakerBuilder.Build();
        var photo = new PortfolioPhoto { Id = Guid.NewGuid(), StorageKey = "photo.jpg", ContentType = "image/jpeg" };
        item.Photos.Add(photo);
        var repository = PortfolioTestData.Repository(item);
        Expression<Func<PortfolioEvent, bool>>? predicate = null;
        repository.Setup(service => service.GetFirstAsync(It.IsAny<Expression<Func<PortfolioEvent, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<PortfolioEvent, object>>[]>()))
            .Callback<Expression<Func<PortfolioEvent, bool>>, CancellationToken, Expression<Func<PortfolioEvent, object>>[]>((filter, token, joins) => predicate = filter).ReturnsAsync(item);
        var storage = new Mock<IPhotoStorage>();
        var stream = new MemoryStream();
        using var source = new CancellationTokenSource();
        storage.Setup(service => service.OpenAsync("photo.jpg", source.Token)).ReturnsAsync(stream);
        var result = await new ReadEventPhotoHandler(repository.Object, storage.Object).Handle(new ReadEventPhotoQuery { EventId = item.IdExterno, PhotoId = photo.Id }, source.Token);
        Assert.False(predicate!.Compile()(item));
        item.SetVisibility(true, Guid.NewGuid()); Assert.True(predicate.Compile()(item));
        Assert.Same(stream, result.Content); Assert.Equal("image/jpeg", result.ContentType);
    }
    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => new ReadEventPhotoHandler(new Mock<IRepository<PortfolioEvent>>().Object,
            new Mock<IPhotoStorage>().Object).Handle(null!, CancellationToken.None));
    }
}
