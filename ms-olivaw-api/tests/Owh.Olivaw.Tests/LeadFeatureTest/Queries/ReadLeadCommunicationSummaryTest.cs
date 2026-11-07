using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Api.Features.LeadFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest.Queries;
public class ReadLeadCommunicationSummaryTest
{
    [Fact]
    public async Task Handle_should_count_real_communication_states_and_expose_sample_limit()
    {
        using var cancellation=new CancellationTokenSource();
        var repository=new Mock<IRepository<Lead>>();
        var snapshot="{\"entry\":\"prospecting\",\"email\":\"contact@example.test\",\"contactDiscovery\":{\"status\":\"not-found\"},\"communications\":[{\"kind\":\"prepared\",\"intentId\":\"a\"},{\"kind\":\"sending\",\"intentId\":\"a\"},{\"kind\":\"sent\",\"intentId\":\"a\"},{\"kind\":\"prepared\",\"intentId\":\"b\"},{\"kind\":\"sending\",\"intentId\":\"b\"},{\"kind\":\"suppressed\",\"intentId\":\"a\"}]}";
        var lead=Lead.Create(Guid.NewGuid(),new string('a',64),1,snapshot,DateTime.UtcNow);
        var rows=Enumerable.Repeat(lead,501).ToArray();
        repository.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<Lead,bool>>>(),501,It.IsAny<Expression<Func<Lead,object>>>(),cancellation.Token,It.IsAny<Expression<Func<Lead,object>>[]>())).ReturnsAsync(rows);
        var response=await new ReadLeadCommunicationSummaryHandler(repository.Object).Handle(new(),cancellation.Token);
        Assert.True(response.Truncated);
        Assert.Equal(500,response.Inspected);
        Assert.Equal(500,response.Counts["collected"]);
        Assert.Equal(500,response.Counts["noEmail"]);
        Assert.Equal(500,response.Counts["sent"]);
        Assert.Equal(500,response.Counts["pending"]);
        Assert.Equal(0,response.Counts["prepared"]);
        Assert.Equal(0,response.Counts["ready"]);
        repository.VerifyAll();
    }
}
