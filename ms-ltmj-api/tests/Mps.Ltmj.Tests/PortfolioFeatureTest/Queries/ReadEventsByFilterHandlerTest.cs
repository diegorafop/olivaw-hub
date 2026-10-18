using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Mps.Ltmj.Domain;
using Mps.OlivawCore.Repositories;
using Mps.Ltmj.Tests.Mocks;
using Mps.Ltmj.Api.Features.PortfolioFeature.Queries;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest.Queries;

public class ReadEventsByFilterHandlerTest
{
    [Fact]
    public async Task Handle_should_read_public_page_and_map_photos()
    {
        var item = PortfolioEventFakerBuilder.Build();
        item.SetVisibility(true, Guid.NewGuid());
        var photoId = Guid.NewGuid();
        item.Photos.Add(new PortfolioPhoto { ExternalId = photoId, Position = 0 });
        var repository = new Mock<IRepository<PortfolioEvent>>();
        using var source = new CancellationTokenSource();
        repository.Setup(service => service.GetAsync(
                It.IsAny<Expression<Func<PortfolioEvent, bool>>>(), source.Token,
                It.IsAny<Expression<Func<PortfolioEvent, object>>[]>()))
            .ReturnsAsync(new[] { item });
        var result = await new ReadEventsByFilterHandler(repository.Object).Handle(new ReadEventsByFilterQuery(), source.Token);
        var response = Assert.Single(result.Items);
        Assert.Equal(item.ExternalId, response.Id); Assert.Equal(item.Title, response.Title);
        Assert.Equal(item.Description, response.Description); Assert.Equal(item.Category, response.Category);
        Assert.Equal($"portfolio/events/{item.ExternalId}/photos/{photoId}", Assert.Single(response.Photos).Url);
        repository.VerifyAll();
    }
    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => new ReadEventsByFilterHandler(new Mock<IRepository<PortfolioEvent>>().Object).Handle(null!, CancellationToken.None));
    }
}
