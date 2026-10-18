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
using Mps.Ltmj.Api.Features.PortfolioFeature.Commands;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest.Commands;

public class UpdateEventHandlerTest
{
    [Fact]
    public async Task Handle_should_persist_and_propagate_cancellation()
    {
        var item = PortfolioEventFakerBuilder.Build();
        var repository = PortfolioTestData.Repository(item);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var response = await new UpdateEventHandler(repository.Object).Handle(new UpdateEventCommand { Id = item.IdExterno, Title = "Novo título", Description = "Descrição", Category = "Casamentos" }, token);
        Assert.Equal("Novo título", item.Title);
        Assert.Equal("Casamentos", item.Category);
        repository.Verify(service => service.SaveChangesAsync(token), Times.Once);
    }
    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        var repository = PortfolioTestData.Repository(PortfolioEventFakerBuilder.Build());
        await Assert.ThrowsAsync<ArgumentNullException>(() => new UpdateEventHandler(repository.Object).Handle(null!, CancellationToken.None));
        repository.VerifyNoOtherCalls();
    }
}
