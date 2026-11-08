using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Api.Features.CustomerFeature.Queries;
using Owh.Olivaw.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.CustomerFeatureTest.Queries;
public class ReadCustomerSummaryTest
{
    [Fact]
    public async Task Handle_should_use_database_counts_and_default_absent_status_to_zero()
    {
        var repository=new Mock<IRepositoryCustomer>();using var source=new CancellationTokenSource();
        repository.Setup(item=>item.ReadCountsAsync(source.Token)).ReturnsAsync(new Dictionary<string,long> {["active"]=2,["closed"]=1});
        var result=await new ReadCustomerSummaryHandler(repository.Object).Handle(new ReadCustomerSummaryMediatorQuery(),source.Token);
        Assert.Equal(3,result.Total);Assert.Equal(2,result.Active);Assert.Equal(1,result.Closed);Assert.Equal(0,result.Paused);Assert.Equal(0,result.Onboarding);
        repository.Verify(item=>item.ReadCountsAsync(source.Token),Times.Once);
    }
    [Fact]
    public async Task Handle_should_reject_null_request() => await Assert.ThrowsAsync<ArgumentNullException>(()=>new ReadCustomerSummaryHandler(new Mock<IRepositoryCustomer>().Object).Handle(null!,CancellationToken.None));
}
