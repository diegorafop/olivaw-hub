using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Mps.Ltmj.Domain;
using Mps.Ltmj.Services;
using Mps.OlivawCore.Repositories;
using Mps.Ltmj.Tests.Mocks;
using Mps.Ltmj.Api.Features.PortfolioFeature.Commands;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest.Commands;

public class DeleteEventHandlerTest
{
    [Fact]
    public async Task Handle_should_remove_files_and_event_permanently()
    {
        var item = PortfolioEventFakerBuilder.Build();
        item.Photos.Add(new PortfolioPhoto { StorageKey = "photo.jpg" });
        var repository = PortfolioTestData.Repository(item);
        var storage = new Mock<IPhotoStorage>();
        using var source = new CancellationTokenSource();
        await new DeleteEventHandler(repository.Object, storage.Object).Handle(new DeleteEventCommand { Id = item.ExternalId }, source.Token);
        storage.Verify(service => service.DeleteAsync("photo.jpg", source.Token), Times.Once);
        repository.Verify(service => service.RemoveAsync(item, source.Token), Times.Once);
        repository.Verify(service => service.SaveChangesAsync(source.Token), Times.Exactly(2));
    }
    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => new DeleteEventHandler(new Mock<IRepository<PortfolioEvent>>().Object,
            new Mock<IPhotoStorage>().Object).Handle(null!, CancellationToken.None));
    }
}
