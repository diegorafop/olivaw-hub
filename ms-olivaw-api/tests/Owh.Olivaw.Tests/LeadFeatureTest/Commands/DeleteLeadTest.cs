using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest.Commands;

public class DeleteLeadTest
{
    private readonly Lead _lead = Lead.Create(Guid.NewGuid(), new string('a',64),1,"{\"revision\":1,\"communications\":[{\"kind\":\"sent\"}]}",DateTime.UtcNow);
    private readonly Mock<IRepository<Lead>> _repository = new();
    private readonly Mock<IRepository<LeadChange>> _changes = new();
    private DeleteLeadMediatorCommand Request() => new() { ExternalId = _lead.ExternalId, ActorUserId = Guid.NewGuid(), ActorName = "Operador", Payload = new() { Revision = 1, FollowUpRevision = 0 } };
    private DeleteLeadHandler Handler(CancellationToken token)
    {
        _repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Lead,bool>>>(),token,It.IsAny<Expression<Func<Lead,object>>[]>())).ReturnsAsync(_lead);
        return new(_repository.Object,_changes.Object);
    }
    [Fact]
    public async Task Handle_should_soft_delete_preserve_snapshot_and_make_retry_idempotent()
    {
        using var cancellation = new CancellationTokenSource();
        var command = Request(); var previous = _lead.Snapshot;
        var handler = Handler(cancellation.Token);
        var result = await handler.Handle(command,cancellation.Token);
        Assert.False(result.Conflict); Assert.NotNull(_lead.DeletedAt);
        Assert.Equal(previous,_lead.Snapshot); Assert.Equal("discontinued",_lead.FollowUpStatus);
        Assert.Equal(command.ActorUserId,_lead.ActorUserId);
        Assert.True((await handler.Handle(command,cancellation.Token)).AlreadyDeleted);
        _repository.Verify(item => item.UpdateAsync(_lead,cancellation.Token),Times.Once);
        _repository.Verify(item => item.SaveChangesAsync(cancellation.Token),Times.Once);
        _changes.Verify(item => item.AddAsync(It.IsAny<LeadChange>(),cancellation.Token),Times.Once);
    }
    [Theory]
    [InlineData(2,0)]
    [InlineData(1,1)]
    public async Task Handle_should_reject_stale_revision_without_deleting(long revision,long followUpRevision)
    {
        var handler=Handler(CancellationToken.None); var command=Request();
        command.Payload.Revision=revision;command.Payload.FollowUpRevision=followUpRevision;
        Assert.True((await handler.Handle(command,CancellationToken.None)).Conflict);
        Assert.Null(_lead.DeletedAt);
        _repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()),Times.Never);
    }
    [Fact]
    public async Task Handle_should_translate_database_concurrency_and_reject_null_request()
    {
        var handler=Handler(CancellationToken.None);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!,CancellationToken.None));
        _repository.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ThrowsAsync(new DbUpdateConcurrencyException());
        Assert.True((await handler.Handle(Request(),CancellationToken.None)).Conflict);
    }
    [Fact]
    public void Validator_should_require_actor_and_revision_vector()
    {
        var command=Request();var validator=new DeleteLeadMediatorValidator();
        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
        command.ActorUserId=Guid.Empty;validator.TestValidate(command).ShouldHaveValidationErrorFor(item => item.ActorUserId);
        Assert.False(new DeleteLeadValidator().Validate(new DeleteLeadCommand { Revision=0,FollowUpRevision=-1 }).IsValid);
    }
}
