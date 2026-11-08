using System;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Api.Features.SecurityRemediationFeature.Commands;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.SecurityRemediationFeatureTest.Commands;
public sealed class UpdateSecurityRemediationTest
{
    [Fact]
    public async Task Handle_should_require_correct_executor_token_phase_and_health_before_success()
    {
        var ticket = SecurityRemediationFakerBuilder.Ticket("authorized");
        var handler = new UpdateSecurityRemediationHandler(InventoryFakerBuilder.Repository(ticket).Object);
        var request = new UpdateSecurityRemediationMediatorCommand { ExternalId = ticket.ExternalId, ActorUserId = ticket.ExecutorUserId, Payload = new() { Action = "claim", LeaseToken = Guid.NewGuid() } };
        Assert.Equal("executing", (await handler.Handle(request, CancellationToken.None)).Status);
        Assert.Equal(ticket.HostId, ticket.ActiveHostKey);
        request.Payload.Action = "resolved";
        Assert.True((await handler.Handle(request, CancellationToken.None)).Conflict);
        request.Payload.Action = "validating";
        Assert.Equal("validating", (await handler.Handle(request, CancellationToken.None)).Status);
        request.Payload.Action = "resolved";
        Assert.True((await handler.Handle(request, CancellationToken.None)).Conflict);
        request.Payload.ResultJson = "{\"healthBefore\":{\"healthy\":true},\"healthAfter\":{\"healthy\":true,\"samples\":2},\"findingAbsent\":true,\"snapshotUnchanged\":true}";
        Assert.Equal("resolved", (await handler.Handle(request, CancellationToken.None)).Status);
        Assert.Null(ticket.ActiveHostKey);
        request.Payload.LeaseToken = Guid.NewGuid();
        Assert.True((await handler.Handle(request, CancellationToken.None)).Conflict);
        request.ActorUserId = Guid.NewGuid();
        Assert.True((await handler.Handle(request, CancellationToken.None)).Conflict);
    }
    [Fact]
    public async Task Handle_should_not_claim_expired_approval_and_should_accept_intervention_after_expired_lease()
    {
        var ticket = SecurityRemediationFakerBuilder.Ticket("authorized");ticket.ApprovalExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        var handler = new UpdateSecurityRemediationHandler(InventoryFakerBuilder.Repository(ticket).Object);
        var request = new UpdateSecurityRemediationMediatorCommand { ExternalId = ticket.ExternalId, ActorUserId = ticket.ExecutorUserId, Payload = new() { Action = "claim", LeaseToken = ticket.LeaseToken!.Value } };
        Assert.True((await handler.Handle(request, CancellationToken.None)).Conflict);
        ticket.Status = "executing";ticket.ActiveHostKey = ticket.HostId;ticket.LeaseExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        request.Payload.Action = "intervention";
        Assert.Equal("intervention", (await handler.Handle(request, CancellationToken.None)).Status);Assert.Null(ticket.ActiveHostKey);
    }
    [Theory]
    [InlineData("rm -rf /", "{}")]
    [InlineData("resolved", "not-json")]
    public void Validate_should_reject_unknown_operation_or_invalid_evidence(string action, string json)
    {
        var request = new UpdateSecurityRemediationMediatorCommand { ExternalId = Guid.NewGuid(), ActorUserId = Guid.NewGuid(), Payload = new() { Action = action, LeaseToken = Guid.NewGuid(), ResultJson = json } };
        Assert.False(new UpdateSecurityRemediationValidator().Validate(request).IsValid);
    }
    [Fact]
    public async Task Handle_should_abandon_unclaimed_authorization_without_enabling_a_mutation_retry()
    {
        var ticket = SecurityRemediationFakerBuilder.Ticket("authorized");ticket.LeaseToken = null;
        var handler = new UpdateSecurityRemediationHandler(InventoryFakerBuilder.Repository(ticket).Object);
        var request = new UpdateSecurityRemediationMediatorCommand { ExternalId = ticket.ExternalId, ActorUserId = ticket.ExecutorUserId, Payload = new() { Action = "intervention", LeaseToken = Guid.NewGuid() } };
        Assert.Equal("intervention", (await handler.Handle(request, CancellationToken.None)).Status);
        request.Payload.Action = "claim";
        Assert.True((await handler.Handle(request, CancellationToken.None)).Conflict);
    }
    [Fact]
    public async Task Handle_should_hold_host_reservation_when_execution_cannot_be_reconstructed_safely()
    {
        var ticket = SecurityRemediationFakerBuilder.Ticket("executing");ticket.ActiveHostKey = ticket.HostId;
        var handler = new UpdateSecurityRemediationHandler(InventoryFakerBuilder.Repository(ticket).Object);
        var request = new UpdateSecurityRemediationMediatorCommand { ExternalId = ticket.ExternalId, ActorUserId = ticket.ExecutorUserId, Payload = new() { Action = "intervention", LeaseToken = ticket.LeaseToken!.Value, ResultJson = "{\"executionUncertain\":true}" } };
        Assert.Equal("intervention", (await handler.Handle(request, CancellationToken.None)).Status);
        Assert.Equal(ticket.HostId,ticket.ActiveHostKey);
    }
    [Fact]
    public async Task Review_should_require_manual_confirmation_and_expired_lease_without_claiming_resolved()
    {
        var ticket = SecurityRemediationFakerBuilder.Ticket("intervention");ticket.ActiveHostKey = ticket.HostId;ticket.ResultJson = "{}";
        var handler = new UpdateSecurityRemediationReviewHandler(InventoryFakerBuilder.Repository(ticket).Object);
        var request = new UpdateSecurityRemediationReviewMediatorCommand { ExternalId = ticket.ExternalId, ActorUserId = Guid.NewGuid(), Payload = new() { ConfirmedStopped = true, Note = "Processo encerrado e infraestrutura conferida manualmente." } };
        Assert.True((await handler.Handle(request, CancellationToken.None)).Conflict);
        ticket.LeaseExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        request.Payload.ConfirmedStopped = false;
        Assert.True((await handler.Handle(request, CancellationToken.None)).Conflict);
        request.Payload.ConfirmedStopped = true;
        Assert.Equal("intervention", (await handler.Handle(request, CancellationToken.None)).Status);
        Assert.Null(ticket.ActiveHostKey);Assert.Contains("operatorReview",ticket.ResultJson);
    }
}
