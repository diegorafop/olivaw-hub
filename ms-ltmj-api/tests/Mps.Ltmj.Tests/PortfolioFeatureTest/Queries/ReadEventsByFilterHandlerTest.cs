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

public class ReadEventsByFilterHandlerTest
{
    [Fact]
    public async Task Handle_should_read_public_page_and_map_photos()
    {
        var item = PortfolioEventFakerBuilder.Build();
        var photoId = Guid.NewGuid();
        item.Photos.Add(new PortfolioPhoto { Id = photoId, Position = 0 });
        var reader = new Mock<IPortfolioReader>();
        using var source = new CancellationTokenSource();
        reader.Setup(service => service.ListAsync(true, 2, 12, source.Token)).ReturnsAsync(new[] { item });
        var result = await new ReadEventsByFilterHandler(reader.Object).Handle(new ReadEventsByFilterQuery { Page = 2 }, source.Token);
        var response = Assert.Single(result.Items);
        Assert.Equal(item.IdExterno, response.Id); Assert.Equal(item.Title, response.Title);
        Assert.Equal(item.Description, response.Description); Assert.Equal(item.Category, response.Category);
        Assert.Equal($"portfolio/events/{item.IdExterno}/photos/{photoId}", Assert.Single(response.Photos).Url);
        reader.VerifyAll();
    }
    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => new ReadEventsByFilterHandler(new Mock<IPortfolioReader>().Object).Handle(null!, CancellationToken.None));
    }
}
