using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest.Commands;

public class UpdateLeadFollowUpTest
{
    [Fact]
    public async Task Handle_should_persist_audited_change_and_preserve_public_snapshot()
    {
        var lead = LeadFollowUpFakerBuilder.Lead();
        var command = LeadFollowUpFakerBuilder.Command(lead);
        using var cancellation = new CancellationTokenSource();
        var repository = Repository(lead, cancellation.Token);
        var changes = new Mock<IRepository<LeadChange>>();
        LeadChange? saved = null;
        changes.Setup(item => item.AddAsync(It.IsAny<LeadChange>(), cancellation.Token))
            .Callback<LeadChange, CancellationToken>((change, _) => saved = change).Returns(Task.CompletedTask);
        var result = await new UpdateLeadFollowUpHandler(repository.Object, changes.Object).Handle(command, cancellation.Token);
        Assert.False(result.Conflict);
        Assert.Equal(1, result.Response.Revision);
        Assert.Equal("conversation", lead.FollowUpStatus);
        Assert.Equal("{}", lead.Snapshot);
        Assert.Equal("submitted", lead.Stage);
        Assert.Equal(1, lead.Revision);
        Assert.Null(lead.UpdatedAt);
        Assert.Equal(command.ActorUserId, saved!.ActorUserId);
        Assert.Equal("Operador", saved.ActorName);
        Assert.Equal("new", saved.PreviousStatus);
        Assert.Equal("Aguardando fotos.", saved.Note);
        Assert.Equal(lead.FollowUpUpdatedAt, saved.CreatedAt);
        repository.Verify(item => item.UpdateAsync(lead, cancellation.Token), Times.Once);
        repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Once);
        var retry = await new UpdateLeadFollowUpHandler(repository.Object, changes.Object).Handle(command, cancellation.Token);
        Assert.True(retry.Conflict);
        changes.Verify(item => item.AddAsync(It.IsAny<LeadChange>(), cancellation.Token), Times.Once);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_should_return_conflict_for_database_race(bool duplicateHistory)
    {
        var lead = LeadFollowUpFakerBuilder.Lead();
        var repository = Repository(lead, CancellationToken.None);
        DbUpdateException failure = new DbUpdateConcurrencyException();
        if (duplicateHistory) failure = new DbUpdateException("Conflito", new PostgresException("Conflito", "ERROR", "ERROR", "23505"));
        repository.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ThrowsAsync(failure);
        var result = await new UpdateLeadFollowUpHandler(repository.Object, Mock.Of<IRepository<LeadChange>>())
            .Handle(LeadFollowUpFakerBuilder.Command(lead), CancellationToken.None);
        Assert.True(result.Conflict);
    }
    [Fact]
    public async Task Handle_should_reject_no_change_and_null_request()
    {
        var lead = LeadFollowUpFakerBuilder.Lead();
        var request = LeadFollowUpFakerBuilder.Command(lead);
        request.Payload.Status = "new"; request.Payload.Note = "";
        var repository = Repository(lead, CancellationToken.None);
        var handler = new UpdateLeadFollowUpHandler(repository.Object, Mock.Of<IRepository<LeadChange>>());
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
        repository.Verify(item => item.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
    [Theory]
    [InlineData("new")]
    [InlineData("conversation")]
    [InlineData("analysis")]
    [InlineData("development")]
    [InlineData("validation")]
    [InlineData("delivered")]
    [InlineData("discontinued")]
    public void Domain_should_allow_every_transition_and_preserve_follow_up_on_public_resubmission(string status)
    {
        foreach (var previous in new[] { "new", "conversation", "analysis", "development", "validation", "delivered", "discontinued" })
        {
            var lead = LeadFollowUpFakerBuilder.Lead();
            lead.UpdateFollowUp(previous, "Primeira nota", LeadFollowUpFakerBuilder.Actor, "Operador", LeadFollowUpFakerBuilder.Now);
            var change = lead.UpdateFollowUp(status, "Segunda nota", LeadFollowUpFakerBuilder.Actor, "Operador", LeadFollowUpFakerBuilder.Now);
            lead.Apply(2, "{\"business\":\"Atualizado\"}", LeadFollowUpFakerBuilder.Now.AddMinutes(1));
            Assert.Equal(status, lead.FollowUpStatus);
            Assert.Equal(2, lead.FollowUpRevision);
            Assert.Equal(previous, change.PreviousStatus);
            Assert.Equal(status, change.Status);
        }
    }
    [Theory]
    [InlineData("status")]
    [InlineData("revision")]
    [InlineData("note")]
    [InlineData("actor")]
    [InlineData("name")]
    [InlineData("id")]
    [InlineData("payload")]
    public void Validator_should_reject_invalid_payload_without_payload_prefix(string field)
    {
        var request = LeadFollowUpFakerBuilder.Command(LeadFollowUpFakerBuilder.Lead());
        new UpdateLeadFollowUpMediatorValidator().TestValidate(request).ShouldNotHaveAnyValidationErrors();
        switch (field)
        {
            case "status": request.Payload.Status = "invalid"; break;
            case "revision": request.Payload.Revision = -1; break;
            case "note": request.Payload.Note = new string('x', 3001); break;
            case "actor": request.ActorUserId = Guid.Empty; break;
            case "name": request.ActorName = ""; break;
            case "id": request.ExternalId = Guid.Empty; break;
            case "payload": request.Payload = null!; break;
        }
        var result = new UpdateLeadFollowUpMediatorValidator().Validate(request);
        Assert.False(result.IsValid);
        Assert.All(result.Errors, error => Assert.DoesNotContain("Payload.", error.PropertyName));
    }
    private static Mock<IRepository<Lead>> Repository(Lead lead, CancellationToken token)
    {
        var repository = new Mock<IRepository<Lead>>();
        repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Lead, bool>>>(), token, It.IsAny<Expression<Func<Lead, object>>[]>()))
            .Callback<Expression<Func<Lead, bool>>, CancellationToken, Expression<Func<Lead, object>>[]>((filter, _, _) =>
            {
                Assert.True(filter.Compile()(lead));
                var other = LeadFollowUpFakerBuilder.Lead();
                Assert.False(filter.Compile()(other));
                lead.DeletedAt = LeadFollowUpFakerBuilder.Now;
                Assert.False(filter.Compile()(lead));
                lead.DeletedAt = null;
            }).ReturnsAsync(lead);
        return repository;
    }
}
