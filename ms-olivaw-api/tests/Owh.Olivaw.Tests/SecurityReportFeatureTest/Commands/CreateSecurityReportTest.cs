using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using Owh.Olivaw.Api.Features.SecurityReportFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;
namespace Owh.Olivaw.Tests.SecurityReportFeatureTest.Commands;
public sealed class CreateSecurityReportTest
{
    [Fact]
    public async Task Handle_should_preserve_raw_json_actor_and_cancel_token()
    {
        var repository = new Mock<IRepository<SecurityReport>>();
        using var source = new CancellationTokenSource();
        var request = new CreateSecurityReportMediatorCommand { Payload = SecurityReportFakerBuilder.Create(), ActorUserId = SecurityReportFakerBuilder.Actor };
        var result = await new CreateSecurityReportHandler(repository.Object).Handle(request, source.Token);
        Assert.NotEqual(Guid.Empty, result.ExternalId);
        Assert.False(result.AlreadyReceived);
        repository.Verify(item => item.AddAsync(It.Is<SecurityReport>(entity => entity.RawData == request.Payload.RawData && entity.ActorUserId == request.ActorUserId && entity.HostId == "test-host"), source.Token), Times.Once);
        repository.Verify(item => item.SaveChangesAsync(source.Token), Times.Once);
    }
    [Fact]
    public async Task Handle_should_acknowledge_replay_without_duplicate_insert()
    {
        var repository = new Mock<IRepository<SecurityReport>>();
        var existing = SecurityReportFakerBuilder.Entity();
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<SecurityReport, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var request = new CreateSecurityReportMediatorCommand { Payload = SecurityReportFakerBuilder.Create(), ActorUserId = existing.ActorUserId };
        var handler = new CreateSecurityReportHandler(repository.Object);
        Assert.True((await handler.Handle(request, CancellationToken.None)).AlreadyReceived);
        request.ActorUserId = Guid.NewGuid();
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
        repository.Verify(item => item.AddAsync(It.IsAny<SecurityReport>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task Handle_should_reject_invalid_envelope_and_null_request()
    {
        var handler = new CreateSecurityReportHandler(new Mock<IRepository<SecurityReport>>().Object);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(new(), CancellationToken.None));
    }
    [Theory]
    [InlineData("schemaVersion", "1.0", "2.0")]
    [InlineData("agentId", "baley", "other")]
    [InlineData("executionStatus", "partial", "safe")]
    [InlineData("completedAt", "2026-01-01T00:01:00Z", "2025-01-01T00:00:00Z")]
    public void Validate_should_reject_invalid_stable_envelope(string field, string oldValue, string newValue)
    {
        var request = SecurityReportFakerBuilder.Create();
        request.RawData = request.RawData.Replace(oldValue, newValue);
        Assert.False(new CreateSecurityReportValidator().Validate(request).IsValid);
        Assert.NotEmpty(field);
    }
    [Fact]
    public void Validate_should_accept_unknown_result_fields_but_reject_ambiguous_or_oversized_json()
    {
        var command = SecurityReportFakerBuilder.Create();
        var validator = new CreateSecurityReportValidator();
        Assert.True(validator.Validate(command).IsValid);
        command.RawData = command.RawData.Replace("{\"schemaVersion\"", "{\"agentId\":\"baley\",\"schemaVersion\"");
        Assert.False(validator.Validate(command).IsValid);
        command.RawData = new string('x', 262145);
        Assert.False(validator.Validate(command).IsValid);
        Assert.False(new CreateSecurityReportMediatorValidator().Validate(new CreateSecurityReportMediatorCommand { Payload = SecurityReportFakerBuilder.Create() }).IsValid);
    }
}
