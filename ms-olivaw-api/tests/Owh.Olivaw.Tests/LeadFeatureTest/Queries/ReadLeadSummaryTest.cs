using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Moq;
using Owh.Olivaw.Api.Features.LeadFeature.Queries;
using Owh.Olivaw.Client.Features.LeadFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest.Queries;
public class ReadLeadSummaryTest
{
    [Fact]
    public async Task Handle_should_return_measured_counts_with_seven_day_window()
    {
        using var cancellation = new CancellationTokenSource();
        var reader = new Mock<IRepositoryLead>();
        reader.Setup(item => item.ReadSummaryAsync(It.Is<DateTime>(date => date.Kind == DateTimeKind.Utc), cancellation.Token)).ReturnsAsync(new LeadReadSummary { Total = 31, Recent = 4 });
        var response = await new ReadLeadSummaryHandler(reader.Object).Handle(new(), cancellation.Token);
        Assert.Equal(31, response.Total);
        Assert.Equal(4, response.Recent);
        Assert.Equal(TimeSpan.FromDays(7), response.AsOf - response.Since);
        reader.VerifyAll();
    }
}
