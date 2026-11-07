using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest.Commands;

public class UpdateLeadCommunicationTest
{
    private readonly Lead _lead = Lead.Create(Guid.NewGuid(), new string('a', 64), 1,
        "{\"revision\":1,\"email\":\"contact@example.test\",\"acquisition\":{\"initiative\":\"outbound\"},\"qualification\":{\"tags\":[]},\"requirements\":\"Preservar\"}", DateTime.UtcNow);
    private readonly Mock<IRepository<Lead>> _repository = new();
    private readonly Mock<IRepository<LeadChange>> _changes = new();
    private readonly Guid _actor = Guid.NewGuid();
    private UpdateLeadCommunicationHandler Handler(CancellationToken token = default)
    {
        _repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Lead, bool>>>(), token, It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(_lead);
        return new(_repository.Object, _changes.Object);
    }
    private UpdateLeadCommunicationMediatorCommand Request(string kind, char identity, string messageId = "", string threadId = "", string replyMessageId = "") => new()
    {
        ExternalId = _lead.ExternalId, ActorUserId = _actor, ActorName = "Trevize",
        Payload = new()
        {
            Revision = _lead.Revision, FollowUpRevision = _lead.FollowUpRevision,
            Event = new()
            {
                EventId = new string(identity, 64), IntentId = new string('b', 64), ContentHash = new string('c', 64),
                Kind = kind, Email = "contact@example.test", Subject = "Parceria Olivaw", BodyPreview = "Texto aprovado",
                GmailMessageId = messageId, ThreadId = threadId, ReplyMessageId = replyMessageId
            }
        }
    };
    [Fact]
    public async Task Handle_should_reserve_send_record_history_and_deduplicate_stale_retries()
    {
        using var cancellation = new CancellationTokenSource();
        var handler = Handler(cancellation.Token);
        var prepared = Request("prepared", 'a');
        Assert.False((await handler.Handle(prepared, cancellation.Token)).Repeated);
        Assert.Equal("new", _lead.FollowUpStatus);
        Assert.True((await handler.Handle(prepared, cancellation.Token)).Repeated);
        await handler.Handle(Request("sending", 'd'), cancellation.Token);
        var sent = Request("sent", 'e', "gmail1", "thread1");
        await handler.Handle(sent, cancellation.Token);
        Assert.Equal("conversation", _lead.FollowUpStatus);
        Assert.True((await handler.Handle(sent, cancellation.Token)).Repeated);
        using var snapshot = JsonDocument.Parse(_lead.Snapshot);
        Assert.Equal("Preservar", snapshot.RootElement.GetProperty("requirements").GetString());
        var records = snapshot.RootElement.GetProperty("communications");
        Assert.Equal(3, records.GetArrayLength());
        Assert.False(records[0].TryGetProperty("bodyPreview", out _));
        _changes.Verify(item => item.AddAsync(It.IsAny<LeadChange>(), cancellation.Token), Times.Exactly(3));
        _repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Exactly(3));
        Assert.Equal(_actor, _lead.ActorUserId);
    }
    [Fact]
    public async Task Handle_should_reject_changed_duplicate_stale_revision_and_second_initial_intent()
    {
        var handler = Handler();
        var request = Request("prepared", 'a');
        await handler.Handle(request, CancellationToken.None);
        request.Payload.Event.Subject = "Outro texto";
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
        var stale = Request("sending", 'd'); stale.Payload.Revision = 1;
        Assert.True((await handler.Handle(stale, CancellationToken.None)).Conflict);
        var second = Request("prepared", 'f'); second.Payload.Event.IntentId = new string('f', 64);
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(second, CancellationToken.None));
        Assert.Equal(2, _lead.Revision);
    }
    [Theory]
    [InlineData("sending")]
    [InlineData("sent")]
    [InlineData("received")]
    [InlineData("human-takeover")]
    public async Task Handle_should_reject_events_without_required_previous_state(string kind)
    {
        var handler = Handler();
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(Request(kind, 'a', "gmail1", "thread1"), CancellationToken.None));
        _repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Theory]
    [InlineData("analysis")]
    [InlineData("discontinued")]
    [InlineData("delivered")]
    public async Task Handle_should_block_reservations_for_leads_outside_pilot_stages(string status)
    {
        _lead.UpdateFollowUp(status, "Intervenção", _actor, "Operador", DateTime.UtcNow);
        var handler = Handler();
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(Request("prepared", 'a'), CancellationToken.None));
    }
    [Fact]
    public async Task Handle_should_block_unknown_recipient_optout_and_changed_approved_content()
    {
        var handler = Handler();
        var unknown = Request("prepared", 'a'); unknown.Payload.Event.Email = "other@example.test";
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(unknown, CancellationToken.None));
        var snapshot = System.Text.Json.Nodes.JsonNode.Parse(_lead.Snapshot)!;
        snapshot["qualification"]!["tags"] = new System.Text.Json.Nodes.JsonArray("opt-out");
        _lead.Apply(2, snapshot.ToJsonString(), DateTime.UtcNow);
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(Request("prepared", 'a'), CancellationToken.None));
        snapshot["qualification"]!["tags"] = new System.Text.Json.Nodes.JsonArray();
        _lead.Apply(3, snapshot.ToJsonString(), DateTime.UtcNow);
        await handler.Handle(Request("prepared", 'a'), CancellationToken.None);
        var changed = Request("sending", 'd'); changed.Payload.Event.ContentHash = new string('f', 64);
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(changed, CancellationToken.None));
    }
    [Fact]
    public async Task Handle_should_record_received_and_human_takeover_without_regressing_stage()
    {
        var handler = Handler();
        await handler.Handle(Request("prepared", 'a'), CancellationToken.None);
        await handler.Handle(Request("sending", 'd'), CancellationToken.None);
        await handler.Handle(Request("sent", 'e', "sent1", "thread1"), CancellationToken.None);
        _lead.UpdateFollowUp("analysis", "Avançou", _actor, "Operador", DateTime.UtcNow);
        await handler.Handle(Request("received", 'f', "reply1", "thread1"), CancellationToken.None);
        Assert.Equal("analysis", _lead.FollowUpStatus);
        await handler.Handle(Request("human-takeover", '1', "human1", "thread1"), CancellationToken.None);
        _lead.UpdateFollowUp("conversation", "Voltou", _actor, "Operador", DateTime.UtcNow);
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(Request("prepared", '2', threadId: "thread1", replyMessageId: "reply1"), CancellationToken.None));
    }
    [Fact]
    public async Task Handle_should_allow_only_one_reply_to_a_recorded_received_message()
    {
        var handler = Handler();
        await handler.Handle(Request("prepared", 'a'), CancellationToken.None);
        await handler.Handle(Request("sending", 'd'), CancellationToken.None);
        await handler.Handle(Request("sent", 'e', "sent1", "thread1"), CancellationToken.None);
        await handler.Handle(Request("received", 'f', "reply1", "thread1"), CancellationToken.None);
        var reply = Request("prepared", '1', threadId: "thread1", replyMessageId: "reply1");
        reply.Payload.Event.IntentId = new string('1', 64);
        await handler.Handle(reply, CancellationToken.None);
        var second = Request("prepared", '2', threadId: "thread1", replyMessageId: "reply1");
        second.Payload.Event.IntentId = new string('2', 64);
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(second, CancellationToken.None));
    }
    [Fact]
    public async Task Handle_should_report_database_concurrency_and_reject_null()
    {
        var handler = Handler();
        _repository.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ThrowsAsync(new DbUpdateConcurrencyException());
        Assert.True((await handler.Handle(Request("prepared", 'a'), CancellationToken.None)).Conflict);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
    [Fact]
    public void Validator_should_enforce_bounded_safe_events_actor_and_revisions()
    {
        var request = Request("prepared", 'a');
        new UpdateLeadCommunicationMediatorValidator().TestValidate(request).ShouldNotHaveAnyValidationErrors();
        request.Payload.Event.Kind = "sent";
        request.Payload.Event.Email = "x@example.test\r\nBCC: x@evil.test";
        request.Payload.Event.Subject = "Unsafe\nSubject";
        request.Payload.Event.BodyPreview = new string('x', 1501);
        request.Payload.Event.IntentId = "invalid";
        var result = new UpdateLeadCommunicationValidator().TestValidate(request.Payload);
        result.ShouldHaveValidationErrorFor(item => item.Event.Email);
        result.ShouldHaveValidationErrorFor(item => item.Event.Subject);
        result.ShouldHaveValidationErrorFor(item => item.Event.BodyPreview);
        result.ShouldHaveValidationErrorFor(item => item.Event.IntentId);
        result.ShouldHaveValidationErrorFor(item => item.Event.GmailMessageId);
        result.ShouldHaveValidationErrorFor(item => item.Event.ThreadId);
        request.ActorUserId = Guid.Empty; request.ActorName = ""; request.ExternalId = Guid.Empty;
        var mediator = new UpdateLeadCommunicationMediatorValidator().TestValidate(request);
        mediator.ShouldHaveValidationErrorFor(item => item.ActorUserId);
        mediator.ShouldHaveValidationErrorFor(item => item.ActorName);
        mediator.ShouldHaveValidationErrorFor(item => item.ExternalId);
    }
}
