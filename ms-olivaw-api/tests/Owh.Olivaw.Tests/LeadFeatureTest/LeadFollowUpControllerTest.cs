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
    public async Task Create_should_forward_authenticated_actor_and_translate_duplicate(bool conflict)
    {
        using var cancellation = new CancellationTokenSource();
        var actorId = Guid.NewGuid();
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", actorId.ToString())], "test")) };
        var actor = new CurrentActor(new HttpContextAccessor { HttpContext = context });
        var payload = new Client.Features.LeadFeature.Commands.CreateLeadCommand { ExternalId = Guid.NewGuid() };
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.Is<CreateLeadMediatorCommand>(request => request.Payload == payload && request.ActorUserId == actorId), cancellation.Token))
            .ReturnsAsync(new Client.Features.LeadFeature.Commands.CreateLeadResponse { ExternalId = payload.ExternalId, Conflict = conflict });
        var result = await new LeadReadController(mediator.Object).Create(payload, actor, cancellation.Token);
        if (conflict) Assert.IsType<ConflictObjectResult>(result);
        else Assert.Equal($"/leads/{payload.ExternalId}", Assert.IsType<CreatedResult>(result).Location);
        mediator.VerifyAll();
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateProposal_should_forward_actor_revision_and_token(bool conflict)
    {
        using var cancellation = new CancellationTokenSource();
        var actorId = Guid.NewGuid(); var leadId = Guid.NewGuid();
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", actorId.ToString())], "test")) };
        var actor = new CurrentActor(new HttpContextAccessor { HttpContext = context });
        var payload = new Client.Features.LeadFeature.Commands.UpdateLeadProposalCommand { Revision = 4 };
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.Is<UpdateLeadProposalMediatorCommand>(request => request.Payload == payload && request.ActorUserId == actorId && request.ExternalId == leadId), cancellation.Token))
            .ReturnsAsync(new Client.Features.LeadFeature.Commands.UpdateLeadProposalResponse { Revision = 5, Conflict = conflict });
        var result = await new LeadReadController(mediator.Object).UpdateProposal(leadId, payload, actor, cancellation.Token);
        if (conflict) Assert.IsType<ConflictObjectResult>(result);
        else Assert.Equal(5, Assert.IsType<Client.Features.LeadFeature.Commands.UpdateLeadProposalResponse>(Assert.IsType<OkObjectResult>(result).Value).Revision);
        mediator.VerifyAll();
    }

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
