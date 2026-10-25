using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Api.Features.SecurityRemediationFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.SecurityRemediationFeatureTest.Commands;
public sealed class CreateSecurityRemediationTest
{
    [Fact]
    public async Task Handle_should_authorize_fixed_plan_and_preserve_existing_approval()
    {
        var report = SecurityRemediationFakerBuilder.Report();
        var reports = InventoryFakerBuilder.Repository(report); var repository = InventoryFakerBuilder.Repository<SecurityRemediation>();
        var request = new CreateSecurityRemediationMediatorCommand { ActorUserId = Guid.NewGuid(), Payload = new() { ReportExternalId = report.ExternalId, FindingIndex = 0 } };
        using var source = new CancellationTokenSource();
        var handler = new CreateSecurityRemediationHandler(reports.Object, repository.Object);
        var first = await handler.Handle(request, source.Token);
        Assert.Equal("authorized", first.Status);
        Assert.Equal(first.ExternalId, (await handler.Handle(request, source.Token)).ExternalId);
        repository.Verify(item => item.AddAsync(It.Is<SecurityRemediation>(row => row.ExecutorUserId == report.ActorUserId && row.ActorUserId == request.ActorUserId && row.PlanJson.Contains("authorizedKeys")), source.Token), Times.Once);
    }
    [Theory]
    [InlineData(1, false)]
    [InlineData(0, true)]
    public async Task Handle_should_require_intervention_for_unknown_plan_or_stale_report(int index, bool stale)
    {
        var report = SecurityRemediationFakerBuilder.Report(); if (stale) report.CompletedAt = DateTime.UtcNow.AddDays(-2);
        var result = await new CreateSecurityRemediationHandler(InventoryFakerBuilder.Repository(report).Object, InventoryFakerBuilder.Repository<SecurityRemediation>().Object)
            .Handle(new() { ActorUserId = Guid.NewGuid(), Payload = new() { ReportExternalId = report.ExternalId, FindingIndex = index } }, CancellationToken.None);
        Assert.Equal("intervention", result.Status);
    }
    [Fact]
    public async Task Batch_should_skip_existing_items_and_save_once_with_safe_and_manual_plans()
    {
        var report = SecurityRemediationFakerBuilder.Report(); var repository = InventoryFakerBuilder.Repository<SecurityRemediation>();
        List<SecurityRemediation>? captured = null;
        repository.Setup(item => item.AddCollectionAsync(It.IsAny<IEnumerable<SecurityRemediation>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<SecurityRemediation>, CancellationToken>((items, token) => captured = new(items)).Returns(Task.CompletedTask);
        var response = await new CreateSecurityRemediationBatchHandler(InventoryFakerBuilder.Repository(report).Object, repository.Object)
            .Handle(new() { ActorUserId = Guid.NewGuid(), Payload = new() { ReportExternalId = report.ExternalId } }, CancellationToken.None);
        Assert.Equal(1, response.AuthorizedCount); Assert.Equal(1, response.InterventionCount); Assert.Equal(2, captured!.Count);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        var existing = SecurityRemediationFakerBuilder.Ticket("executing"); existing.FindingIndex = 0;
        repository = InventoryFakerBuilder.Repository(existing);
        repository.Setup(item => item.AddCollectionAsync(It.IsAny<IEnumerable<SecurityRemediation>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        response = await new CreateSecurityRemediationBatchHandler(InventoryFakerBuilder.Repository(report).Object, repository.Object)
            .Handle(new() { ActorUserId = Guid.NewGuid(), Payload = new() { ReportExternalId = report.ExternalId } }, CancellationToken.None);
        Assert.Equal(1, response.AlreadyHandledCount); Assert.Equal(0, response.AuthorizedCount); Assert.Equal("executing", existing.Status);
    }
    [Fact]
    public void Validators_should_reject_missing_actor_and_target()
    {
        Assert.False(new CreateSecurityRemediationValidator().Validate(new CreateSecurityRemediationMediatorCommand()).IsValid);
        Assert.False(new CreateSecurityRemediationBatchValidator().Validate(new CreateSecurityRemediationBatchMediatorCommand()).IsValid);
    }
}
