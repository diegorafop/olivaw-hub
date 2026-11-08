using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.ManagedServiceFeatureTest.Commands;

public class CreateManagedServiceTest
{
    [Fact]
    public async Task Handle_should_create_shared_service_and_audit_without_recreating_customers()
    {
        using var cancellation = new CancellationTokenSource();
        var customers = new[] { ManagedServiceFakerBuilder.Customer(1), ManagedServiceFakerBuilder.Customer(2) };
        var command = ManagedServiceFakerBuilder.Command();
        command.Payload.Scope = "customer";
        command.Payload.CustomerExternalIds = customers.Select(item => item.ExternalId).ToList();
        var repository = new Mock<IRepository<ManagedService>>();
        var customerRepository = new Mock<IRepository<Customer>>();
        customerRepository.Setup(item => item.GetAsync(It.IsAny<Expression<Func<Customer, bool>>>(), cancellation.Token, It.IsAny<Expression<Func<Customer, object>>[]>())).ReturnsAsync(customers);
        ManagedService? added = null;
        repository.Setup(item => item.AddAsync(It.IsAny<ManagedService>(), cancellation.Token)).Callback<ManagedService, CancellationToken>((item, _) => added = item).Returns(Task.CompletedTask);
        var result = await new CreateManagedServiceHandler(repository.Object, customerRepository.Object).Handle(command, cancellation.Token);
        Assert.Null(result.Conflict);
        Assert.Equal(added!.ExternalId, result.Response.ExternalId);
        Assert.Equal(2, added.Customers.Count);
        Assert.All(added.Customers, item => Assert.Equal("Operador", item.StartedByName));
        Assert.Equal(command.ActorUserId, Assert.Single(added.Changes).ActorUserId);
        repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Once);
        customerRepository.Verify(item => item.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task Handle_should_reject_missing_customer_before_writing()
    {
        var command = ManagedServiceFakerBuilder.Command();
        command.Payload.Scope = "customer"; command.Payload.CustomerExternalIds.Add(Guid.NewGuid());
        var repository = new Mock<IRepository<ManagedService>>();
        var customers = new Mock<IRepository<Customer>>();
        customers.Setup(item => item.GetAsync(It.IsAny<Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Customer, object>>[]>())).ReturnsAsync(Array.Empty<Customer>());
        var handler = new CreateManagedServiceHandler(repository.Object, customers.Object);
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
        repository.Verify(item => item.AddAsync(It.IsAny<ManagedService>(), It.IsAny<CancellationToken>()), Times.Never);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
    [Fact]
    public async Task Handle_should_return_conflict_for_existing_identifier_and_race()
    {
        var repository = new Mock<IRepository<ManagedService>>();
        repository.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<ManagedService, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new CreateManagedServiceHandler(repository.Object, Mock.Of<IRepository<Customer>>());
        Assert.NotNull((await handler.Handle(ManagedServiceFakerBuilder.Command(), CancellationToken.None)).Conflict);
        repository.Verify(item => item.AddAsync(It.IsAny<ManagedService>(), It.IsAny<CancellationToken>()), Times.Never);
        repository.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<ManagedService, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateConcurrencyException());
        Assert.NotNull((await handler.Handle(ManagedServiceFakerBuilder.Command(), CancellationToken.None)).Conflict);
    }
    [Theory]
    [InlineData("id")]
    [InlineData("type")]
    [InlineData("status")]
    [InlineData("scope")]
    [InlineData("internalLinks")]
    [InlineData("duplicateLinks")]
    [InlineData("noCustomer")]
    [InlineData("actor")]
    public void Validator_should_reject_invalid_contract(string field)
    {
        var request = ManagedServiceFakerBuilder.Command();
        Assert.True(new CreateManagedServiceMediatorValidator().Validate(request).IsValid);
        switch (field)
        {
            case "id": request.Payload.TechnicalId = "Bad_ID"; break;
            case "type": request.Payload.Type = "database"; break;
            case "status": request.Payload.Status = "healthy"; break;
            case "scope": request.Payload.Scope = "unknown"; break;
            case "internalLinks": request.Payload.CustomerExternalIds.Add(Guid.NewGuid()); break;
            case "duplicateLinks": request.Payload.Scope = "customer"; var id = Guid.NewGuid(); request.Payload.CustomerExternalIds.AddRange(new[] { id, id }); break;
            case "noCustomer": request.Payload.Scope = "customer"; break;
            case "actor": request.ActorUserId = Guid.Empty; break;
        }
        var result = new CreateManagedServiceMediatorValidator().Validate(request);
        Assert.False(result.IsValid);
        Assert.All(result.Errors, error => Assert.DoesNotContain("Payload.", error.PropertyName));
    }
}
