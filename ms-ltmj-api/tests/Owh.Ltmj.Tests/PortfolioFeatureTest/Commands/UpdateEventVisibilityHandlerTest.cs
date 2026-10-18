using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Owh.Ltmj.Domain;
using Owh.OlivawCore.Repositories;
using Owh.Ltmj.Tests.Mocks;
using Owh.Ltmj.Api.Features.PortfolioFeature.Commands;

namespace Owh.Ltmj.Tests.PortfolioFeatureTest.Commands;

public class UpdateEventVisibilityHandlerTest
{
    [Fact]
    public async Task Handle_should_persist_and_propagate_cancellation()
    {
        var item = PortfolioEventFakerBuilder.Build();
        var repository = PortfolioTestData.Repository(item);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var response = await new UpdateEventVisibilityHandler(repository.Object).Handle(new UpdateEventVisibilityCommand { Id = item.ExternalId, IsVisible = true }, token);
        Assert.True(item.IsVisible);
        repository.Verify(service => service.SaveChangesAsync(token), Times.Once);
    }
    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        var repository = PortfolioTestData.Repository(PortfolioEventFakerBuilder.Build());
        await Assert.ThrowsAsync<ArgumentNullException>(() => new UpdateEventVisibilityHandler(repository.Object).Handle(null!, CancellationToken.None));
        repository.VerifyNoOtherCalls();
    }
}
