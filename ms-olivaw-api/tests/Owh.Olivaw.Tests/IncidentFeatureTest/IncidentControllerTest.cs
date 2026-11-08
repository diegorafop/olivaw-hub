using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
using Owh.Olivaw.Api.Features.IncidentFeature;
using Owh.Olivaw.Api.Features.IncidentFeature.Commands;
using Owh.Olivaw.Api.Features.IncidentFeature.Queries;
using Owh.Olivaw.Client.Features.IncidentFeature.Commands;
using Owh.Olivaw.Client.Features.IncidentFeature.Queries;
namespace Owh.Olivaw.Tests.IncidentFeatureTest;
public sealed class IncidentControllerTest
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Create_should_forward_public_payload_actor_and_token_and_translate_conflict(bool conflict)
    {
        using var cancellation = new CancellationTokenSource();
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", InventoryFakerBuilder.Actor.ToString()), new Claim("name", "Operador") }, "test")) };
        var mediator = new Mock<IMediator>(); var payload = new CreateIncidentCommand();
        mediator.Setup(item => item.Send(It.Is<CreateIncidentMediatorCommand>(request => request.Payload == payload && request.ActorUserId == InventoryFakerBuilder.Actor && request.ActorName == "Operador"), cancellation.Token)).ReturnsAsync(new CreateIncidentResult { Conflict = conflict ? "Conflito" : null });
        var controller = new IncidentController(mediator.Object, new CurrentActor(new HttpContextAccessor { HttpContext = context })) { ControllerContext = new() { HttpContext = context } };
        var result = await controller.Create(payload, cancellation.Token);
        if (conflict) Assert.IsType<ConflictObjectResult>(result); else Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
        mediator.VerifyAll();
    }
    [Fact]
    public async Task Read_should_forward_query_and_token()
    {
        using var cancellation = new CancellationTokenSource();
        var mediator = new Mock<IMediator>(); var payload = new ReadIncidentByFilterQuery();
        mediator.Setup(item => item.Send(It.Is<ReadIncidentByFilterMediatorQuery>(request => request.Payload == payload), cancellation.Token)).ReturnsAsync(new ReadIncidentByFilterResponse());
        var controller = new IncidentController(mediator.Object, new CurrentActor(new HttpContextAccessor()));
        Assert.IsType<OkObjectResult>(await controller.Read(payload, cancellation.Token)); mediator.VerifyAll();
    }
    [Fact]
    public async Task Update_and_history_should_forward_identity_revision_and_token()
    {
        using var cancellation = new CancellationTokenSource(); var id = Guid.NewGuid(); var mediator = new Mock<IMediator>(); var payload = new UpdateIncidentCommand { Revision = 2 };
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", InventoryFakerBuilder.Actor.ToString()) }, "test")) };
        mediator.Setup(item => item.Send(It.Is<UpdateIncidentMediatorCommand>(request => request.ExternalId == id && request.Payload == payload && request.ActorUserId == InventoryFakerBuilder.Actor), cancellation.Token)).ReturnsAsync(new UpdateIncidentResult());
        mediator.Setup(item => item.Send(It.Is<ReadIncidentHistoryMediatorQuery>(request => request.Payload.ExternalId == id && request.Payload.BeforeId == 10), cancellation.Token)).ReturnsAsync(new ReadIncidentHistoryResponse());
        var controller = new IncidentController(mediator.Object, new CurrentActor(new HttpContextAccessor { HttpContext = context })) { ControllerContext = new() { HttpContext = context } };
        Assert.IsType<OkObjectResult>(await controller.Update(id, payload, cancellation.Token)); Assert.IsType<OkObjectResult>(await controller.ReadHistory(id, 10, cancellation.Token)); mediator.VerifyAll();
    }
}
