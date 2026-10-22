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
public class ReadLeadByFilterTest
{
    [Fact]
    public async Task Handle_should_delegate_pagination_filters_and_token()
    {
        using var cancellation = new CancellationTokenSource();
        var reader = new Mock<IRepositoryLead>();
        var from = DateTimeOffset.Parse("2026-10-20T00:00:00-03:00");
        var payload = new ReadLeadByFilterQuery { Page = 2, PageSize = 10, Search = "Pastelaria", Entry = "studio", From = from };
        var id = Guid.NewGuid();
        reader.Setup(item => item.ReadAsync(It.Is<LeadReadFilter>(filter => filter.Page == 2 && filter.PageSize == 10 && filter.Search == "Pastelaria" && filter.Entry == "studio" && filter.From == from.UtcDateTime), cancellation.Token))
            .ReturnsAsync(new LeadReadPage { Total = 11, Items = new List<LeadReadItem> { new() { ExternalId = id, Business = "Pastelaria", Whatsapp = "11987654321" } } });
        var response = await new ReadLeadByFilterHandler(reader.Object).Handle(new ReadLeadByFilterMediatorQuery { Payload = payload }, cancellation.Token);
        Assert.Equal(11, response.Total);
        Assert.Equal(2, response.Page);
        Assert.Equal(id, Assert.Single(response.Items).ExternalId);
        Assert.Equal("11987654321", response.Items[0].Whatsapp);
        reader.VerifyAll();
    }
    [Theory]
    [InlineData(0, 20, null)]
    [InlineData(1, 101, null)]
    [InlineData(1, 0, null)]
    [InlineData(1, 20, "invalid")]
    public void Validator_should_reject_unbounded_or_invalid_filters(int page, int size, string? entry)
    {
        var result = new ReadLeadByFilterValidator().Validate(new ReadLeadByFilterQuery { Page = page, PageSize = size, Entry = entry });
        Assert.False(result.IsValid);
    }
    [Fact]
    public void Validator_should_reject_reversed_dates_and_long_search()
    {
        var payload = new ReadLeadByFilterQuery { Search = new string('a', 101), From = DateTimeOffset.UtcNow, Until = DateTimeOffset.UtcNow.AddDays(-1) };
        var result = new ReadLeadByFilterValidator().TestValidate(payload);
        result.ShouldHaveValidationErrorFor(item => item.Search);
        result.ShouldHaveValidationErrorFor(item => item.Until);
        new ReadLeadByFilterValidator().TestValidate(new ReadLeadByFilterQuery()).ShouldNotHaveAnyValidationErrors();
        Assert.False(new ReadLeadByFilterMediatorValidator().Validate(new ReadLeadByFilterMediatorQuery { Payload = null! }).IsValid);
    }
}
