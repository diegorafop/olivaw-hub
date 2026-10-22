using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.LeadFeature;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Api.Features.LeadFeature.Queries;
using Owh.Olivaw.Client.Features.LeadFeature.Queries;
using Owh.Olivaw.Tests.Mocks;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest;

public class LeadFollowUpControllerTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_should_use_authenticated_actor_and_translate_conflict(bool conflict)
    {
        var lead = LeadFollowUpFakerBuilder.Lead();
        var command = LeadFollowUpFakerBuilder.Command(lead);
        using var cancellation = new CancellationTokenSource();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", command.ActorUserId.ToString()), new Claim("name", command.ActorName) }, "test"))
        };
        var actor = new CurrentActor(new HttpContextAccessor { HttpContext = context });
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.Is<UpdateLeadFollowUpMediatorCommand>(request => request.ExternalId == command.ExternalId && request.Payload == command.Payload && request.ActorUserId == command.ActorUserId && request.ActorName == command.ActorName), cancellation.Token))
            .ReturnsAsync(new UpdateLeadFollowUpResult { Conflict = conflict, Response = new() { Revision = 1 } });
        var controller = new LeadReadController(mediator.Object) { ControllerContext = new ControllerContext { HttpContext = context } };
        var result = await controller.UpdateFollowUp(command.ExternalId, command.Payload, actor, cancellation.Token);
        if (conflict) Assert.IsType<ConflictObjectResult>(result);
        else Assert.Equal(1, Assert.IsType<Client.Features.LeadFeature.Commands.UpdateLeadFollowUpResponse>(Assert.IsType<OkObjectResult>(result).Value).Revision);
        mediator.VerifyAll();
    }
    [Fact]
    public async Task ReadHistory_should_forward_id_cursor_and_token()
    {
        var id = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var response = new ReadLeadHistoryResponse { NextBeforeRevision = 5 };
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.Is<ReadLeadHistoryMediatorQuery>(request => request.Payload.ExternalId == id && request.Payload.BeforeRevision == 25), cancellation.Token)).ReturnsAsync(response);
        var result = await new LeadReadController(mediator.Object).ReadHistory(id, 25, cancellation.Token);
        Assert.Same(response, Assert.IsType<OkObjectResult>(result).Value);
        mediator.VerifyAll();
    }
}
