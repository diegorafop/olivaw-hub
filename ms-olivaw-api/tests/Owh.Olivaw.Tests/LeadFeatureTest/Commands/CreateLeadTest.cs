using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Moq;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Xunit;
namespace Owh.Olivaw.Tests.LeadFeatureTest.Commands;
public class CreateLeadTest
{
    private static CreateLeadCommand Payload() => new() { ExternalId = Guid.NewGuid(), Origin = "99Freelas", Title = "Comentários", Requirements = "Reações e comentários" };
    [Fact]
    public async Task Handle_should_create_outbound_without_contact_and_refuse_duplicate_identifier()
    {
        using var cancellation = new CancellationTokenSource();
        var repository = new Mock<IRepository<Lead>>();
        Lead? saved = null;
        repository.Setup(item => item.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Lead, bool>>>(), cancellation.Token, It.IsAny<Expression<Func<Lead, object>>[]>())).ReturnsAsync(() => saved!);
        repository.Setup(item => item.AddAsync(It.IsAny<Lead>(), cancellation.Token)).Callback<Lead, CancellationToken>((lead, _) => saved = lead).Returns(Task.CompletedTask);
        var handler = new CreateLeadHandler(repository.Object);
        var command = new CreateLeadMediatorCommand { Payload = Payload(), ActorUserId = Guid.NewGuid() };
        var response = await handler.Handle(command, cancellation.Token);
        Assert.False(response.Conflict); Assert.Equal(command.Payload.ExternalId, response.ExternalId);
        Assert.Equal(command.ActorUserId, saved!.ActorUserId);
        using var snapshot = JsonDocument.Parse(saved.Snapshot);
        Assert.Equal("prospecting", snapshot.RootElement.GetProperty("entry").GetString());
        Assert.Equal("outbound", snapshot.RootElement.GetProperty("acquisition").GetProperty("initiative").GetString());
        Assert.Equal("", snapshot.RootElement.GetProperty("email").GetString());
        var repeated = await handler.Handle(command, cancellation.Token);
        Assert.True(repeated.Conflict);
        repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Once);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
    [Fact]
    public void Validator_should_allow_unknown_contact_and_reject_unsafe_origin_url_and_invalid_fields()
    {
        var payload = Payload(); var validator = new CreateLeadValidator();
        validator.TestValidate(payload).ShouldNotHaveAnyValidationErrors();
        payload.SourceUrl = "javascript:alert(1)"; payload.Email = "invalid"; payload.Whatsapp = "123";
        payload.Initiative = "other"; payload.Title = ""; payload.Requirements = "";
        var result = validator.TestValidate(payload);
        result.ShouldHaveValidationErrorFor(item => item.SourceUrl); result.ShouldHaveValidationErrorFor(item => item.Email);
        result.ShouldHaveValidationErrorFor(item => item.Whatsapp); result.ShouldHaveValidationErrorFor(item => item.Initiative);
        result.ShouldHaveValidationErrorFor(item => item.Title); result.ShouldHaveValidationErrorFor(item => item.Requirements);
        new CreateLeadMediatorValidator().TestValidate(new CreateLeadMediatorCommand { Payload = null! }).ShouldHaveValidationErrorFor(string.Empty);
    }
}
