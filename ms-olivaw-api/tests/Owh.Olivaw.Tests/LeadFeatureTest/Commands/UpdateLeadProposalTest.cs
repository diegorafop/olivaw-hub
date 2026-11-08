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
public class UpdateLeadProposalTest
{
    private static UpdateLeadProposalCommand Payload() => new() { Revision = 1, Proposal = new() { Title = "Comentários", Rows = [new() { Description = "Integrar", Role = "Programador", Hours = 10, HourlyRate = 100 }] } };
    private static Mock<IRepository<Lead>> Repository(Lead? lead, CancellationToken token)
    {
        var repository = new Mock<IRepository<Lead>>();
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), token, It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(lead!);
        return repository;
    }
    [Fact]
    public async Task Handle_should_preserve_original_requirements_and_prevent_stale_overwrite()
    {
        using var cancellation = new CancellationTokenSource();
        var lead = Lead.Create(Guid.NewGuid(), new string('a', 64), 1, "{\"revision\":1,\"requirements\":\"Original\",\"futureField\":true}", DateTime.UtcNow);
        var repository = Repository(lead, cancellation.Token);
        var handler = new UpdateLeadProposalHandler(repository.Object);
        var request = new UpdateLeadProposalMediatorCommand { ExternalId = lead.ExternalId, ActorUserId = Guid.NewGuid(), Payload = Payload() };
        var response = await handler.Handle(request, cancellation.Token);
        Assert.False(response.Conflict); Assert.Equal(2, response.Revision);
        using var saved = JsonDocument.Parse(lead.Snapshot);
        Assert.Equal("Original", saved.RootElement.GetProperty("requirements").GetString());
        Assert.True(saved.RootElement.GetProperty("futureField").GetBoolean());
        Assert.Equal("Comentários", saved.RootElement.GetProperty("proposal").GetProperty("title").GetString());
        Assert.True((await handler.Handle(request, cancellation.Token)).Conflict);
        repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Once);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
    [Fact]
    public async Task Handle_should_report_missing_lead_and_database_conflict()
    {
        var request = new UpdateLeadProposalMediatorCommand { ExternalId = Guid.NewGuid(), Payload = Payload() };
        await Assert.ThrowsAsync<ValidationException>(() => new UpdateLeadProposalHandler(Repository(null, CancellationToken.None).Object).Handle(request, CancellationToken.None));
        var lead = Lead.Create(request.ExternalId, new string('a', 64), 1, "{\"revision\":1}", DateTime.UtcNow);
        var repository = Repository(lead, CancellationToken.None);
        repository.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ThrowsAsync(new DbUpdateConcurrencyException());
        Assert.True((await new UpdateLeadProposalHandler(repository.Object).Handle(request, CancellationToken.None)).Conflict);
    }
    [Fact]
    public void Validator_should_reject_invalid_prices_rows_and_conditions()
    {
        var payload = Payload(); var validator = new UpdateLeadProposalValidator();
        validator.TestValidate(payload).ShouldNotHaveAnyValidationErrors();
        payload.Proposal.Rows[0].Hours = -1; payload.Proposal.Rows[0].HourlyRate = 0;
        payload.Proposal.DiscountPercent = 101; payload.Proposal.ReservePercent = -1; payload.Proposal.Installments = 0;
        payload.Proposal.Status = "published";
        var result = validator.TestValidate(payload);
        Assert.Contains(result.Errors, error => error.PropertyName == "Proposal.Rows[0].Hours");
        Assert.Contains(result.Errors, error => error.PropertyName == "Proposal.Rows[0].HourlyRate");
        result.ShouldHaveValidationErrorFor(item => item.Proposal.DiscountPercent);
        result.ShouldHaveValidationErrorFor(item => item.Proposal.ReservePercent);
        result.ShouldHaveValidationErrorFor(item => item.Proposal.Installments);
        result.ShouldHaveValidationErrorFor(item => item.Proposal.Status);
        payload.Proposal.Rows = []; validator.TestValidate(payload).ShouldHaveValidationErrorFor(item => item.Proposal.Rows);
        payload.Proposal = null!; validator.TestValidate(payload).ShouldHaveValidationErrorFor(item => item.Proposal);
    }
}
