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
using Mps.Ltmj.Api.Features.PortfolioFeature.Commands;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest.Commands;

public class CreateEventHandlerTest
{
    [Fact]
    public async Task Handle_should_persist_and_propagate_cancellation()
    {
        var item = PortfolioEventFakerBuilder.Build();
        var repository = PortfolioTestData.Repository(item);
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var response = await new CreateEventHandler(repository.Object).Handle(new CreateEventCommand { Title = "Churrasco", Description = "Descrição", Category = "Confraternizações" }, token);
        repository.Verify(service => service.AddAsync(It.Is<PortfolioEvent>(item => item.Title == "Churrasco" && !item.IsVisible), token), Times.Once);
        Assert.NotEqual(Guid.Empty, response.Id);
        repository.Verify(service => service.SaveChangesAsync(token), Times.Once);
    }
    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        var repository = PortfolioTestData.Repository(PortfolioEventFakerBuilder.Build());
        await Assert.ThrowsAsync<ArgumentNullException>(() => new CreateEventHandler(repository.Object).Handle(null!, CancellationToken.None));
        repository.VerifyNoOtherCalls();
    }
}
