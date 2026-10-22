using System;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest.Commands;

public class UpdateLeadTest
{
    [Fact]
    public async Task Handle_should_create_snapshot_and_propagate_cancellation()
    {
        var request = LeadFeatureFakerBuilder.Create();
        using var cancellation = new CancellationTokenSource();
        var repository = new Mock<IRepository<Lead>>();
        Lead? added = null;
        Expression<Func<Lead, bool>>? predicate = null;
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), cancellation.Token, It.IsAny<Expression<Func<Lead, object>>[]>()))
            .Callback<Expression<Func<Lead, bool>>, CancellationToken, Expression<Func<Lead, object>>[]>((filter, _, _) => predicate = filter)
            .ReturnsAsync((Lead)null!);
        repository.Setup(item => item.AddAsync(It.IsAny<Lead>(), cancellation.Token)).Callback<Lead, CancellationToken>((lead, _) => added = lead).Returns(Task.CompletedTask);
        var result = await new UpdateLeadHandler(repository.Object).Handle(request, cancellation.Token);
        Assert.Equal(LeadWriteOutcome.Saved, result.Outcome);
        Assert.Equal(request.ExternalId, result.Response.ExternalId);
        Assert.Equal(1, result.Response.Revision);
        Assert.Contains("lead@example.test", added!.Snapshot);
        Assert.DoesNotContain(request.WriteKey, added.Snapshot);
        Assert.NotEqual(request.WriteKey, added.WriteKeyHash);
        Assert.True(predicate!.Compile()(added));
        added.DeletedAt = DateTime.UtcNow;
        Assert.False(predicate.Compile()(added));
        repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_should_retry_same_revision_without_duplicate_or_write()
    {
        var repository = new Mock<IRepository<Lead>>();
        Lead? saved = null;
        repository.Setup(item => item.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>())).Callback<Lead, CancellationToken>((lead, _) => saved = lead).Returns(Task.CompletedTask);
        var handler = new UpdateLeadHandler(repository.Object);
        var request = LeadFeatureFakerBuilder.Create();
        await handler.Handle(request, CancellationToken.None);
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(() => saved!);
        saved!.UpdateFollowUp("development", "Nota interna", LeadFollowUpFakerBuilder.Actor, "Operador", LeadFollowUpFakerBuilder.Now);
        var result = await handler.Handle(request, CancellationToken.None);
        Assert.Equal("development", saved.FollowUpStatus);
        Assert.Equal(1, saved.FollowUpRevision);
        Assert.Equal(LeadWriteOutcome.Saved, result.Outcome);
        repository.Verify(item => item.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(true, 2, LeadWriteOutcome.Forbidden)]
    [InlineData(false, 1, LeadWriteOutcome.Conflict)]
    [InlineData(false, 3, LeadWriteOutcome.Saved)]
    public async Task Handle_should_enforce_key_and_revision(bool wrongKey, int revision, LeadWriteOutcome outcome)
    {
        var request = LeadFeatureFakerBuilder.Create();
        request.Payload.Revision = revision;
        if (wrongKey) request.WriteKey = new string('b', 64);
        var lead = Lead.Create(request.ExternalId, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(new string('a', 64)))), 2, "{}", DateTime.UtcNow);
        var repository = new Mock<IRepository<Lead>>();
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(lead);
        var result = await new UpdateLeadHandler(repository.Object).Handle(request, CancellationToken.None);
        Assert.Equal(outcome, result.Outcome);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), outcome == LeadWriteOutcome.Saved ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task Handle_should_return_conflict_for_concurrent_update()
    {
        var request = LeadFeatureFakerBuilder.Create();
        request.Payload.Revision = 2;
        var lead = Lead.Create(request.ExternalId, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.WriteKey))), 1, "{}", DateTime.UtcNow);
        var repository = new Mock<IRepository<Lead>>();
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(lead);
        repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateConcurrencyException());
        Assert.Equal(LeadWriteOutcome.Conflict, (await new UpdateLeadHandler(repository.Object).Handle(request, CancellationToken.None)).Outcome);
    }

    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => new UpdateLeadHandler(Mock.Of<IRepository<Lead>>()).Handle(null!, CancellationToken.None));
    }

    [Fact]
    public void Validator_should_accept_valid_snapshot()
    {
        new UpdateLeadMediatorValidator().TestValidate(LeadFeatureFakerBuilder.Create()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("email")]
    [InlineData("revision")]
    [InlineData("schema")]
    [InlineData("events")]
    [InlineData("requirements")]
    [InlineData("stage")]
    [InlineData("key")]
    [InlineData("project")]
    public void Validator_should_reject_invalid_snapshot(string field)
    {
        var request = LeadFeatureFakerBuilder.Create();
        switch (field)
        {
            case "email": request.Payload.Email = "invalid"; break;
            case "revision": request.Payload.Revision = 0; break;
            case "schema": request.Payload.SchemaVersion = 99; break;
            case "events": request.Payload.Events = new Client.Features.LeadFeature.Commands.LeadJourneyEvent[41]; break;
            case "requirements": request.Payload.Requirements = new string('x', 3001); break;
            case "stage": request.Payload.Stage = "exploring"; break;
            case "key": request.WriteKey = "bad"; break;
            case "project": request.Payload.Project = null; break;
        }
        Assert.False(new UpdateLeadMediatorValidator().Validate(request).IsValid);
    }
    [Theory]
    [InlineData("", "11987654321", true)]
    [InlineData("", null, false)]
    [InlineData("", "", false)]
    [InlineData("", "1198", false)]
    [InlineData("invalid", "11987654321", false)]
    [InlineData("lead@example.test", "1198", false)]
    [InlineData("lead@example.test", "11987654321", true)]
    [InlineData("", "(11) 98765-4321", false)]
    public void Validator_should_require_one_valid_contact_and_validate_both_when_present(string email, string? whatsapp, bool expected)
    {
        var request = LeadFeatureFakerBuilder.Create();
        request.Payload.Email = email;
        request.Payload.Whatsapp = whatsapp;
        Assert.Equal(expected, new UpdateLeadMediatorValidator().Validate(request).IsValid);
    }
    [Fact]
    public void Snapshot_should_preserve_legacy_serialization_when_whatsapp_is_absent()
    {
        var request = LeadFeatureFakerBuilder.Create();
        var json = System.Text.Json.JsonSerializer.Serialize(request.Payload, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        using var document = System.Text.Json.JsonDocument.Parse(json);
        Assert.False(document.RootElement.TryGetProperty("whatsapp", out _));
    }
}
