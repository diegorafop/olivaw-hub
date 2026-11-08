using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Owh.Olivaw.Api.Features.LeadFeature;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Tests.Mocks;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest;

public class LeadControllerTest
{
    [Theory]
    [InlineData(LeadWriteOutcome.Saved, 200)]
    [InlineData(LeadWriteOutcome.Forbidden, 403)]
    [InlineData(LeadWriteOutcome.Conflict, 409)]
    public async Task Update_should_delegate_payload_key_and_cancellation(LeadWriteOutcome outcome, int status)
    {
        var request = LeadFeatureFakerBuilder.Create();
        using var cancellation = new CancellationTokenSource();
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.Is<UpdateLeadMediatorCommand>(command => command.Payload == request.Payload && command.WriteKey == request.WriteKey && command.ExternalId == request.ExternalId), cancellation.Token)).ReturnsAsync(new UpdateLeadResult { Outcome = outcome });
        var result = await new LeadController(mediator.Object).Update(request.ExternalId, request.WriteKey, request.Payload, cancellation.Token);
        Assert.Equal(status, ((Microsoft.AspNetCore.Mvc.Infrastructure.IStatusCodeActionResult)result).StatusCode);
        mediator.VerifyAll();
    }
}
