using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Api.Features.CustomerFeature.Queries;
using Owh.Olivaw.Client.Features.CustomerFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.CustomerFeatureTest.Queries;
public class ReadCustomerByIdTest
{
    [Fact]
    public async Task Handle_should_read_generic_repository_with_contacts_and_bounded_history()
    {
        await using var fixture=await CustomerFixture.StartAsync("root");using var source=new CancellationTokenSource();
        var changes=new Mock<IRepository<CustomerChange>>();
        changes.Setup(item=>item.TakeOrderedAsync(It.IsAny<Expression<Func<CustomerChange,bool>>>(),20,It.IsAny<Expression<Func<CustomerChange,object>>>(),source.Token,It.IsAny<Expression<Func<CustomerChange,object>>[]>())).ReturnsAsync(fixture.Customer.Changes);
        var result=await new ReadCustomerByIdHandler(fixture.Repository.Object,changes.Object).Handle(new ReadCustomerByIdMediatorQuery {Payload=new ReadCustomerByIdQuery {ExternalId=fixture.Customer.ExternalId}},source.Token);
        Assert.Equal(fixture.Customer.ExternalId,result.ExternalId);Assert.Single(result.Contacts);Assert.Single(result.Changes);
        fixture.Repository.Verify(item=>item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Customer,bool>>>(),source.Token,It.IsAny<Expression<Func<Customer,object>>[]>()),Times.Once);
    }
    [Fact]
    public void Validator_should_reject_empty_public_identifier() => Assert.False(new ReadCustomerByIdValidator().Validate(new ReadCustomerByIdQuery()).IsValid);
    [Fact]
    public async Task Handle_should_reject_null_request() => await Assert.ThrowsAsync<ArgumentNullException>(()=>new ReadCustomerByIdHandler(new Mock<IRepository<Customer>>().Object,new Mock<IRepository<CustomerChange>>().Object).Handle(null!,CancellationToken.None));
}
