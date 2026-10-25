using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Api.Features.SecurityReportFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;
namespace Owh.Olivaw.Tests.SecurityReportFeatureTest.Queries;
public sealed class ReadSecurityReportTest
{
    [Fact]
    public async Task Handle_should_bound_list_and_filter_deleted_host_and_cursor()
    {
        var repository = new Mock<IRepository<SecurityReport>>();
        Expression<Func<SecurityReport, bool>>? predicate = null;
        var rows = Enumerable.Range(1, 21).Select(index => { var row = SecurityReportFakerBuilder.Entity(); row.Id = 100-index; return row; }).ToList();
        using var source = new CancellationTokenSource();
        repository.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<SecurityReport, bool>>>(), 21, It.IsAny<Expression<Func<SecurityReport, object>>>(), source.Token))
            .Callback<Expression<Func<SecurityReport, bool>>, int, Expression<Func<SecurityReport, object>>, CancellationToken, Expression<Func<SecurityReport, object>>[]>((filter, count, order, token, joins) => predicate = filter)
            .ReturnsAsync(rows);
        var result = await new ReadSecurityReportByFilterHandler(repository.Object).Handle(new() { Payload = new() { BeforeId = 100, HostId = "test-host" } }, source.Token);
        Assert.Equal(20, result.Items.Count); Assert.Equal(80, result.NextBeforeId);
        var row = rows[0]; Assert.True(predicate!.Compile()(row));
        row.DeletedAt = DateTime.UtcNow; Assert.False(predicate.Compile()(row));
        Assert.False(new ReadSecurityReportByFilterValidator().Validate(new Owh.Olivaw.Client.Features.SecurityReportFeature.Queries.ReadSecurityReportByFilterQuery { BeforeId = -1 }).IsValid);
    }
    [Fact]
    public async Task Handle_should_return_only_existing_detail_and_validate_identifier()
    {
        var repository = new Mock<IRepository<SecurityReport>>();
        var handler = new ReadSecurityReportByIdHandler(repository.Object);
        var row = SecurityReportFakerBuilder.Entity();
        var request = new ReadSecurityReportByIdMediatorQuery { Payload = new() { ExternalId = row.ExternalId } };
        Assert.Null((await handler.Handle(request, CancellationToken.None)).Item);
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<SecurityReport, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(row);
        var response = await handler.Handle(request, CancellationToken.None);
        Assert.Equal(row.RawData, response.Item!.RawData);
        Assert.False(new ReadSecurityReportByIdValidator().Validate(new ReadSecurityReportByIdMediatorQuery()).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
}
