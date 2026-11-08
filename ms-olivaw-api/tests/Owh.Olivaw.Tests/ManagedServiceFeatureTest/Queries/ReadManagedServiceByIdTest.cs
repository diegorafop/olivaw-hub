using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Queries;
using Owh.Olivaw.Client.Features.ManagedServiceFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.ManagedServiceFeatureTest.Queries;
public class ReadManagedServiceByIdTest
{
    [Fact]
    public async Task Handle_should_map_current_and_closed_periods_and_bound_history()
    {
        using var cancellation = new CancellationTokenSource();
        var service = ManagedServiceFakerBuilder.Create("customer");
        var customer = ManagedServiceFakerBuilder.Customer();
        var active = ManagedServiceCustomer.Create(service, customer, ManagedServiceFakerBuilder.Actor, "Operador", ManagedServiceFakerBuilder.Now);
        var closed = ManagedServiceCustomer.Create(service, customer, ManagedServiceFakerBuilder.Actor, "Anterior", ManagedServiceFakerBuilder.Now.AddDays(-2));
        closed.Close(ManagedServiceFakerBuilder.Actor, "Encerramento", ManagedServiceFakerBuilder.Now.AddDays(-1));
        var repository = new Mock<IRepository<ManagedService>>();
        repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<ManagedService, bool>>>(), cancellation.Token, It.IsAny<Expression<Func<ManagedService, object>>[]>())).ReturnsAsync(service);
        var links = new Mock<IRepository<ManagedServiceCustomer>>();
        links.Setup(item => item.GetAsync(It.IsAny<Expression<Func<ManagedServiceCustomer, bool>>>(), cancellation.Token, It.IsAny<Expression<Func<ManagedServiceCustomer, object>>[]>())).ReturnsAsync(new[] { active });
        links.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<ManagedServiceCustomer, bool>>>(), 20, It.IsAny<Expression<Func<ManagedServiceCustomer, object>>>(), cancellation.Token, It.IsAny<Expression<Func<ManagedServiceCustomer, object>>[]>())).ReturnsAsync(new[] { closed });
        var changes = new Mock<IRepository<ManagedServiceChange>>();
        changes.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<ManagedServiceChange, bool>>>(), 20, It.IsAny<Expression<Func<ManagedServiceChange, object>>>(), cancellation.Token, It.IsAny<Expression<Func<ManagedServiceChange, object>>[]>())).ReturnsAsync(service.Changes);
        var handler = new ReadManagedServiceByIdHandler(repository.Object, links.Object, changes.Object);
        var result = await handler.Handle(new() { Payload = new() { ExternalId = service.ExternalId } }, cancellation.Token);
        Assert.Equal(service.TechnicalId, result.TechnicalId);
        Assert.Equal("Operador", Assert.Single(result.Customers).StartedByName);
        Assert.Equal("Encerramento", Assert.Single(result.PreviousCustomers).EndedByName);
        Assert.Equal(1, Assert.Single(result.Changes).Revision);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
        links.VerifyAll(); changes.VerifyAll();
    }
    [Fact]
    public void Validator_should_require_public_id()
    {
        Assert.False(new ReadManagedServiceByIdValidator().Validate(new ReadManagedServiceByIdQuery()).IsValid);
        Assert.True(new ReadManagedServiceByIdValidator().Validate(new ReadManagedServiceByIdQuery { ExternalId = Guid.NewGuid() }).IsValid);
    }
}
