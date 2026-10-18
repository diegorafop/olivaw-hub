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
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using Mps.Ltmj.Api.Features.PortfolioFeature.Commands;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest.Commands;

public class UploadEventPhotosHandlerTest
{
    private static IFormFile Photo(byte first = 0xff)
    {
        var bytes = new byte[] { first, 0xd8, 0xff, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "files", "photo.jpg");
    }
    [Fact]
    public async Task Handle_should_store_multiple_photos_in_order()
    {
        var item = PortfolioEventFakerBuilder.Build();
        var repository = PortfolioTestData.Repository(item);
        var storage = new Mock<IPhotoStorage>();
        storage.Setup(service => service.SaveAsync(It.IsAny<Stream>(), "jpg", It.IsAny<CancellationToken>())).ReturnsAsync("photo.jpg");
        using var source = new CancellationTokenSource();
        var result = await new UploadEventPhotosHandler(repository.Object, storage.Object).Handle(
            new UploadEventPhotosCommand { Id = item.IdExterno, Files = [Photo(), Photo()] }, source.Token);
        Assert.Equal(2, result.Count);
        Assert.Equal(new[] { 0, 1 }, item.Photos.Select(photo => photo.Position));
        storage.Verify(service => service.SaveAsync(It.IsAny<Stream>(), "jpg", source.Token), Times.Exactly(2));
        repository.Verify(service => service.SaveChangesAsync(source.Token), Times.Once);
    }
    [Fact]
    public async Task Handle_should_reject_invalid_signature_before_writing()
    {
        var repository = PortfolioTestData.Repository(PortfolioEventFakerBuilder.Build());
        var storage = new Mock<IPhotoStorage>();
        await Assert.ThrowsAsync<ValidationException>(() => new UploadEventPhotosHandler(repository.Object, storage.Object)
            .Handle(new UploadEventPhotosCommand { Id = Guid.NewGuid(), Files = [Photo(), Photo(0)] }, CancellationToken.None));
        storage.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task Handle_should_cleanup_files_when_database_write_fails()
    {
        var repository = PortfolioTestData.Repository(PortfolioEventFakerBuilder.Build());
        repository.Setup(service => service.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new IOException());
        var storage = new Mock<IPhotoStorage>();
        storage.Setup(service => service.SaveAsync(It.IsAny<Stream>(), "jpg", It.IsAny<CancellationToken>())).ReturnsAsync("photo.jpg");
        await Assert.ThrowsAsync<IOException>(() => new UploadEventPhotosHandler(repository.Object, storage.Object)
            .Handle(new UploadEventPhotosCommand { Id = Guid.NewGuid(), Files = [Photo()] }, CancellationToken.None));
        storage.Verify(service => service.DeleteAsync("photo.jpg", CancellationToken.None), Times.Once);
    }
    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => new UploadEventPhotosHandler(new Mock<IRepository<PortfolioEvent>>().Object,
            new Mock<IPhotoStorage>().Object).Handle(null!, CancellationToken.None));
    }
}
