using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Api.Features.CustomerFeature.Queries;
using Owh.Olivaw.Client.Features.CustomerFeature.Queries;
using Owh.Olivaw.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.CustomerFeatureTest.Queries;
public class ReadCustomerByFilterTest
{
    [Theory]
    [InlineData(0,20,"",false)]
    [InlineData(1,101,"",false)]
    [InlineData(1,20,"active",true)]
    [InlineData(1,20,"invalid",false)]
    public void Validator_should_bound_pagination_and_filter_status(int page,int size,string status,bool valid) => Assert.Equal(valid,new ReadCustomerByFilterValidator().Validate(new ReadCustomerByFilterQuery {Page=page,PageSize=size,Status=status}).IsValid);
    [Fact]
    public async Task Handle_should_forward_filters_and_cancellation_without_loading_all_customers()
    {
        var repository=new Mock<IRepositoryCustomer>();
        using var source=new CancellationTokenSource();
        repository.Setup(item=>item.ReadPageAsync("Aurora","active",2,20,source.Token)).ReturnsAsync(new CustomerReadPage {Total=21});
        var result=await new ReadCustomerByFilterHandler(repository.Object).Handle(new ReadCustomerByFilterMediatorQuery {Payload=new ReadCustomerByFilterQuery {Search="Aurora",Status="active",Page=2}},source.Token);
        Assert.Equal(21,result.Total);Assert.Equal(2,result.Page);Assert.Empty(result.Items);
        repository.Verify(item=>item.ReadPageAsync("Aurora","active",2,20,source.Token),Times.Once);
    }
    [Fact]
    public async Task Handle_should_reject_null_request() => await Assert.ThrowsAsync<ArgumentNullException>(()=>new ReadCustomerByFilterHandler(new Mock<IRepositoryCustomer>().Object).Handle(null!,CancellationToken.None));
}
