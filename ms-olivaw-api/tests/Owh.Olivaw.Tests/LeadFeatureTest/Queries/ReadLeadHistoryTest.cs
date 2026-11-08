using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Api.Features.LeadFeature.Queries;
using Owh.Olivaw.Client.Features.LeadFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest.Queries;

public class ReadLeadHistoryTest
{
    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    [InlineData(21)]
    public async Task Handle_should_bound_history_and_supply_next_cursor(int count)
    {
        var lead = LeadFollowUpFakerBuilder.Lead();
        var rows = Enumerable.Range(0, count).Select(_ => lead.UpdateFollowUp("new", "Nota", LeadFollowUpFakerBuilder.Actor, "Operador", LeadFollowUpFakerBuilder.Now)).Reverse().ToArray();
        using var cancellation = new CancellationTokenSource();
        var repository = new Mock<IRepository<Lead>>();
        repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Lead, bool>>>(), cancellation.Token, It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(lead);
        var changes = new Mock<IRepository<LeadChange>>();
        changes.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<LeadChange, bool>>>(), 21, It.IsAny<Expression<Func<LeadChange, object>>>(), cancellation.Token, It.IsAny<Expression<Func<LeadChange, object>>[]>()))
            .Callback<Expression<Func<LeadChange, bool>>, int, Expression<Func<LeadChange, object>>, CancellationToken, Expression<Func<LeadChange, object>>[]>((filter, _, order, _, _) =>
            {
                foreach (var row in rows) Assert.True(filter.Compile()(row));
                if (rows.Length > 0) Assert.Equal(rows[0].Revision, order.Compile()(rows[0]));
            }).ReturnsAsync(rows);
        var handler = new ReadLeadHistoryHandler(repository.Object, changes.Object);
        var result = await handler.Handle(new() { Payload = new() { ExternalId = lead.ExternalId } }, cancellation.Token);
        Assert.Equal(Math.Min(count, 20), result.Items.Count);
        if (count > 20) Assert.Equal(result.Items.Last().Revision, result.NextBeforeRevision);
        else Assert.Null(result.NextBeforeRevision);
        if (count > 0) Assert.Equal("Nota", result.Items[0].Note);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
    [Fact]
    public async Task Handle_should_filter_older_revisions_and_deleted_changes()
    {
        var lead = LeadFollowUpFakerBuilder.Lead();
        var first = lead.UpdateFollowUp("new", "Nota", LeadFollowUpFakerBuilder.Actor, "Operador", LeadFollowUpFakerBuilder.Now);
        var second = lead.UpdateFollowUp("new", "Outra", LeadFollowUpFakerBuilder.Actor, "Operador", LeadFollowUpFakerBuilder.Now);
        var repository = new Mock<IRepository<Lead>>();
        repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(lead);
        var changes = new Mock<IRepository<LeadChange>>();
        changes.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<LeadChange, bool>>>(), 21, It.IsAny<Expression<Func<LeadChange, object>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<LeadChange, object>>[]>()))
            .Callback<Expression<Func<LeadChange, bool>>, int, Expression<Func<LeadChange, object>>, CancellationToken, Expression<Func<LeadChange, object>>[]>((filter, _, _, _, _) =>
            {
                Assert.True(filter.Compile()(first)); Assert.False(filter.Compile()(second));
                first.DeletedAt = LeadFollowUpFakerBuilder.Now;
                Assert.False(filter.Compile()(first));
                first.DeletedAt = null;
            }).ReturnsAsync(new[] { first });
        var result = await new ReadLeadHistoryHandler(repository.Object, changes.Object).Handle(new() { Payload = new() { ExternalId = lead.ExternalId, BeforeRevision = 2 } }, CancellationToken.None);
        Assert.Equal(1, Assert.Single(result.Items).Revision);
    }
    [Fact]
    public void Validator_should_reject_invalid_id_and_cursor()
    {
        var validator = new ReadLeadHistoryValidator();
        Assert.False(validator.Validate(new ReadLeadHistoryQuery() { ExternalId = Guid.Empty }).IsValid);
        Assert.False(validator.Validate(new ReadLeadHistoryQuery() { ExternalId = Guid.NewGuid(), BeforeRevision = 0 }).IsValid);
        Assert.True(validator.Validate(new ReadLeadHistoryQuery() { ExternalId = Guid.NewGuid(), BeforeRevision = 2 }).IsValid);
        Assert.False(new ReadLeadHistoryMediatorValidator().Validate(new ReadLeadHistoryMediatorQuery() { Payload = null! }).IsValid);
    }
}
