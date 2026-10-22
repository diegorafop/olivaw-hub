using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Moq;
using Owh.Olivaw.Api.Features.LeadFeature.Queries;
using Owh.Olivaw.Client.Features.LeadFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest.Queries;
public class ReadLeadByIdTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_should_preserve_legacy_and_whatsapp_snapshots_without_write_key(bool whatsapp)
    {
        var request = LeadFeatureFakerBuilder.Create();
        if (whatsapp) { request.Payload.Email = ""; request.Payload.Whatsapp = "11987654321"; }
        var snapshot = JsonSerializer.Serialize(request.Payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var lead = Lead.Create(request.ExternalId, new string('a', 64), 1, snapshot, DateTime.UtcNow);
        var repository = new Mock<IRepository<Lead>>();
        Expression<Func<Lead, bool>>? predicate = null;
        using var cancellation = new CancellationTokenSource();
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), cancellation.Token, It.IsAny<Expression<Func<Lead, object>>[]>()))
            .Callback<Expression<Func<Lead, bool>>, CancellationToken, Expression<Func<Lead, object>>[]>((filter, _, _) => predicate = filter).ReturnsAsync(lead);
        var response = await new ReadLeadByIdHandler(repository.Object).Handle(new() { Payload = new() { ExternalId = request.ExternalId } }, cancellation.Token);
        Assert.NotNull(predicate);
        Assert.True(predicate.Compile()(lead));
        lead.DeletedAt = DateTime.UtcNow;
        Assert.False(predicate.Compile()(lead));
        Assert.Equal(request.ExternalId, response.ExternalId);
        Assert.False(response.ContentUnavailable);
        Assert.NotNull(response.Snapshot);
        Assert.Equal(request.Payload.Whatsapp, response.Snapshot.Whatsapp);
        var json = JsonSerializer.Serialize(response);
        Assert.DoesNotContain("WriteKey", json);
        Assert.DoesNotContain("ActorUserId", json);
        repository.VerifyAll();
    }
    [Fact]
    public async Task Handle_should_report_unreadable_content_without_returning_raw_snapshot()
    {
        var lead = Lead.Create(Guid.NewGuid(), new string('a', 64), 1, "not-json", DateTime.UtcNow);
        var repository = new Mock<IRepository<Lead>>();
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(lead);
        var response = await new ReadLeadByIdHandler(repository.Object).Handle(new(), CancellationToken.None);
        Assert.True(response.ContentUnavailable);
        Assert.Null(response.Snapshot);
    }
    [Fact]
    public void Validator_should_require_public_id_and_payload()
    {
        new ReadLeadByIdValidator().TestValidate(new ReadLeadByIdQuery()).ShouldHaveValidationErrorFor(item => item.ExternalId);
        new ReadLeadByIdValidator().TestValidate(new ReadLeadByIdQuery() { ExternalId = Guid.NewGuid() }).ShouldNotHaveAnyValidationErrors();
        Assert.False(new ReadLeadByIdMediatorValidator().Validate(new ReadLeadByIdMediatorQuery { Payload = null! }).IsValid);
    }
}
