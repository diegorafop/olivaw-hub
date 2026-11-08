using System;
using System.Linq;
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

public class UpdateLeadQualificationTest
{
    private static UpdateLeadQualificationMediatorCommand Request() => new()
    {
        ExternalId = Guid.NewGuid(), ActorUserId = Guid.NewGuid(), ActorName = "Operador",
        Payload = new() { Revision = 1, Qualification = new() { Segment = "Eventos", Approach = "Gostaríamos de conversar." } }
    };
    private static Mock<IRepository<Lead>> Repository(Lead? lead, CancellationToken token)
    {
        var repository = new Mock<IRepository<Lead>>();
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), token, It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(lead!);
        return repository;
    }
    [Fact]
    public void Validator_should_accept_unknown_scores_and_require_all_five_evidenced_dimensions()
    {
        var assessment = new LeadWebsiteAssessment { AssessedAt = DateTimeOffset.Parse("2026-11-08T10:00:00Z"), Basis = "simulation", Strategy = "ui" };
        foreach (var dimension in new[] { "modernity", "content", "ui", "ux", "management" })
            assessment.Metrics[dimension] = new() { Evidence = "Não avaliado." };
        assessment.Metrics["ui"] = new() { Score = 5, Evidence = "Simulação: consistência visual pode evoluir.", Confidence = "provisional" };
        var payload = Request().Payload;
        payload.Qualification.WebsiteAssessment = assessment;
        var validator = new UpdateLeadQualificationValidator();
        Assert.True(validator.Validate(payload).IsValid);
        assessment.Metrics["management"].Score = 0;
        Assert.False(validator.Validate(payload).IsValid); // desconhecido não vira nota zero.
        assessment.Metrics["management"].Score = null;
        assessment.Metrics["ui"].Score = 11;
        Assert.False(validator.Validate(payload).IsValid);
        assessment.Metrics["ui"].Score = 5;
        assessment.Metrics["ui"].Evidence = "";
        Assert.False(validator.Validate(payload).IsValid);
        assessment.Metrics.Remove("management");
        Assert.False(validator.Validate(payload).IsValid);
    }
    [Fact]
    public async Task Handle_should_roundtrip_assessment_without_changing_stage_or_existing_data()
    {
        var request = Request();
        request.Payload.Qualification.WebsiteAssessment = new() { AssessedAt = DateTimeOffset.UtcNow, Basis = "simulation", Strategy = "ui" };
        request.Payload.Qualification.WebsiteAssessment.Metrics["ui"] = new() { Score = 5, Evidence = "Simulação", Confidence = "provisional" };
        var lead = Lead.Create(request.ExternalId, new string('a',64),1,"{\"revision\":1,\"requirements\":\"Preservar\"}",DateTime.UtcNow);
        await new UpdateLeadQualificationHandler(Repository(lead,CancellationToken.None).Object).Handle(request,CancellationToken.None);
        using var data = JsonDocument.Parse(lead.Snapshot);
        Assert.Equal(5,data.RootElement.GetProperty("qualification").GetProperty("websiteAssessment").GetProperty("metrics").GetProperty("ui").GetProperty("score").GetInt32());
        Assert.Equal("Preservar",data.RootElement.GetProperty("requirements").GetString());
        Assert.Equal("new",lead.FollowUpStatus);
    }
    [Fact]
    public async Task Handle_should_preserve_original_proposal_unknown_fields_and_record_actor_and_date()
    {
        using var cancellation = new CancellationTokenSource();
        var request = Request();
        var lead = Lead.Create(request.ExternalId, new string('a', 64), 1,
            "{\"revision\":1,\"requirements\":\"Original\",\"proposal\":{\"title\":\"Existente\"},\"futureField\":true}", DateTime.UtcNow);
        var repository = Repository(lead, cancellation.Token);
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), cancellation.Token, It.IsAny<Expression<Func<Lead, object>>[]>()))
            .Returns<Expression<Func<Lead, bool>>, CancellationToken, Expression<Func<Lead, object>>[]>((predicate, _, _) => Task.FromResult(predicate.Compile()(lead) ? lead : null!));
        var before = DateTime.UtcNow;
        var response = await new UpdateLeadQualificationHandler(repository.Object).Handle(request, cancellation.Token);
        Assert.False(response.Conflict);
        Assert.Equal(2, response.Revision);
        Assert.InRange(response.UpdatedAt, before, DateTime.UtcNow);
        Assert.Equal(request.ActorUserId, lead.ActorUserId);
        using var data = JsonDocument.Parse(lead.Snapshot);
        Assert.Equal("Original", data.RootElement.GetProperty("requirements").GetString());
        Assert.Equal("Existente", data.RootElement.GetProperty("proposal").GetProperty("title").GetString());
        Assert.True(data.RootElement.GetProperty("futureField").GetBoolean());
        Assert.Equal("Operador", data.RootElement.GetProperty("qualificationUpdatedBy").GetString());
        Assert.Equal("Eventos", data.RootElement.GetProperty("qualification").GetProperty("segment").GetString());
        Assert.Equal("draft", data.RootElement.GetProperty("qualification").GetProperty("approachStatus").GetString());
        repository.Verify(item => item.UpdateAsync(lead, cancellation.Token), Times.Once);
        repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Once);
    }
    [Fact]
    public async Task Handle_should_reject_stale_revision_without_mutating_snapshot()
    {
        var request = Request();
        var lead = Lead.Create(request.ExternalId, new string('a', 64), 2, "{\"revision\":2}", DateTime.UtcNow);
        var repository = Repository(lead, CancellationToken.None);
        var result = await new UpdateLeadQualificationHandler(repository.Object).Handle(request, CancellationToken.None);
        Assert.True(result.Conflict); Assert.Equal(2, result.Revision);
        Assert.Equal("{\"revision\":2}", lead.Snapshot);
        repository.Verify(item => item.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task Handle_should_report_missing_lead_database_conflict_and_reject_null()
    {
        var request = Request();
        await Assert.ThrowsAsync<ValidationException>(() => new UpdateLeadQualificationHandler(Repository(null, CancellationToken.None).Object).Handle(request, CancellationToken.None));
        var lead = Lead.Create(request.ExternalId, new string('a', 64), 1, "{\"revision\":1}", DateTime.UtcNow);
        var repository = Repository(lead, CancellationToken.None);
        repository.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ThrowsAsync(new DbUpdateConcurrencyException());
        var handler = new UpdateLeadQualificationHandler(repository.Object);
        Assert.True((await handler.Handle(request, CancellationToken.None)).Conflict);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
    [Fact]
    public async Task Handle_should_limit_combined_snapshot_without_persisting_oversized_data()
    {
        var request = Request();
        var lead = Lead.Create(request.ExternalId, new string('a', 64), 1, "{\"existing\":\"" + new string('x', 31900) + "\"}", DateTime.UtcNow);
        var repository = Repository(lead, CancellationToken.None);
        await Assert.ThrowsAsync<ValidationException>(() => new UpdateLeadQualificationHandler(repository.Object).Handle(request, CancellationToken.None));
        Assert.Equal(1, lead.Revision);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task Handle_should_store_public_email_sources_and_contact_form_evidence()
    {
        var request = Request();
        request.Payload.Qualification.PublicEmails = [new() { Email = "contato@example.test", SourceUrl = "https://example.test/contato" }];
        request.Payload.Qualification.ContactPageUrl = "https://example.test/contato";
        request.Payload.Qualification.ContactFormUrl = "https://example.test/contato";
        request.Payload.Qualification.ContactFormStatus = "found";
        request.Payload.Qualification.ContactCheckedAt = DateTimeOffset.UtcNow;
        new UpdateLeadQualificationValidator().TestValidate(request.Payload).ShouldNotHaveAnyValidationErrors();
        var lead = Lead.Create(request.ExternalId, new string('a', 64), 1, "{\"revision\":1}", DateTime.UtcNow);
        await new UpdateLeadQualificationHandler(Repository(lead, CancellationToken.None).Object).Handle(request, CancellationToken.None);
        using var data = JsonDocument.Parse(lead.Snapshot);
        var qualification = data.RootElement.GetProperty("qualification");
        Assert.Equal("contato@example.test", qualification.GetProperty("publicEmails")[0].GetProperty("email").GetString());
        Assert.Equal("https://example.test/contato", qualification.GetProperty("publicEmails")[0].GetProperty("sourceUrl").GetString());
        Assert.Equal("found", qualification.GetProperty("contactFormStatus").GetString());
        Assert.Equal("https://example.test/contato", qualification.GetProperty("contactFormUrl").GetString());
        Assert.Equal(request.Payload.Qualification.ContactCheckedAt, qualification.GetProperty("contactCheckedAt").GetDateTimeOffset());
    }
    [Fact]
    public void Validator_should_reject_invalid_contact_evidence()
    {
        var request = Request().Payload;
        request.Qualification.PublicEmails = [new() { Email = "invalid", SourceUrl = "javascript:alert(1)" }];
        request.Qualification.ContactPageUrl = "file:///tmp/file";
        request.Qualification.ContactFormStatus = "found";
        var result = new UpdateLeadQualificationValidator().TestValidate(request);
        result.ShouldHaveValidationErrorFor("Qualification.PublicEmails[0].Email");
        result.ShouldHaveValidationErrorFor("Qualification.PublicEmails[0].SourceUrl");
        result.ShouldHaveValidationErrorFor(item => item.Qualification.ContactPageUrl);
        result.ShouldHaveValidationErrorFor(item => item.Qualification.ContactFormUrl);
        result.ShouldHaveValidationErrorFor(item => item.Qualification.ContactCheckedAt);
        request.Qualification.PublicEmails = Enumerable.Range(0, 6).Select(index => new LeadPublicEmail { Email = $"e{index}@example.test", SourceUrl = "https://example.test" }).ToArray();
        request.Qualification.ContactFormStatus = "absent";
        result = new UpdateLeadQualificationValidator().TestValidate(request);
        result.ShouldHaveValidationErrorFor(item => item.Qualification.PublicEmails);
        result.ShouldHaveValidationErrorFor(item => item.Qualification.ContactFormStatus);
    }
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://user:pass@example.test")]
    [InlineData("file:///tmp/file")]
    public void Validator_should_reject_unsafe_web_links(string url)
    {
        var request = Request().Payload;
        request.Qualification.WebsiteUrl = url;
        request.Qualification.SourceUrl = url;
        request.Qualification.ContactFormUrl = url;
        var result = new UpdateLeadQualificationValidator().TestValidate(request);
        result.ShouldHaveValidationErrorFor(item => item.Qualification.WebsiteUrl);
        result.ShouldHaveValidationErrorFor(item => item.Qualification.SourceUrl);
        result.ShouldHaveValidationErrorFor(item => item.Qualification.ContactFormUrl);
    }
    [Fact]
    public void Validator_should_accept_safe_draft_and_validate_limits_status_and_actor()
    {
        var request = Request();
        new UpdateLeadQualificationMediatorValidator().TestValidate(request).ShouldNotHaveAnyValidationErrors();
        request.Payload.Qualification.WebsiteUrl = "https://example.test";
        request.Payload.Qualification.Whatsapp = "41999990000";
        new UpdateLeadQualificationValidator().TestValidate(request.Payload).ShouldNotHaveAnyValidationErrors();
        request.Payload.Qualification.ApproachStatus = "sent";
        request.Payload.Qualification.Presence = "missing";
        request.Payload.Qualification.Confidence = "certain";
        request.Payload.Qualification.Priority = "urgent";
        request.Payload.Qualification.Tags = Enumerable.Repeat("evento", 13).ToArray();
        request.Payload.Qualification.Approach = new string('x', 3001);
        request.Payload.Qualification.Whatsapp = "123";
        request.Payload.Qualification.SchemaVersion = 2;
        var errors = new UpdateLeadQualificationValidator().TestValidate(request.Payload);
        errors.ShouldHaveValidationErrorFor(item => item.Qualification.ApproachStatus);
        errors.ShouldHaveValidationErrorFor(item => item.Qualification.Presence);
        errors.ShouldHaveValidationErrorFor(item => item.Qualification.Confidence);
        errors.ShouldHaveValidationErrorFor(item => item.Qualification.Priority);
        errors.ShouldHaveValidationErrorFor(item => item.Qualification.Tags);
        errors.ShouldHaveValidationErrorFor(item => item.Qualification.Approach);
        errors.ShouldHaveValidationErrorFor(item => item.Qualification.Whatsapp);
        errors.ShouldHaveValidationErrorFor(item => item.Qualification.SchemaVersion);
        request.ActorUserId = Guid.Empty; request.ActorName = ""; request.ExternalId = Guid.Empty; request.Payload = null!;
        var mediator = new UpdateLeadQualificationMediatorValidator().TestValidate(request);
        mediator.ShouldHaveValidationErrorFor(item => item.ActorUserId);
        mediator.ShouldHaveValidationErrorFor(item => item.ActorName);
        mediator.ShouldHaveValidationErrorFor(item => item.ExternalId);
        mediator.ShouldHaveValidationErrorFor("");
    }
}
