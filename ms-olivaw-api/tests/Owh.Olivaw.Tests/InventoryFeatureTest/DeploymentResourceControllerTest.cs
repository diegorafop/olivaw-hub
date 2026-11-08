using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.InventoryFeature;
using Owh.Olivaw.Api.Features.InventoryFeature.Commands;
using Owh.Olivaw.Api.Features.InventoryFeature.Queries;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.InventoryFeatureTest;
public sealed class DeploymentResourceControllerTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Commands_should_forward_payload_actor_and_token_and_translate_conflicts(bool conflict)
    {
        using var cancellation = new CancellationTokenSource(); var id = Guid.NewGuid();
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("sub", InventoryFakerBuilder.Actor.ToString()), new Claim("name", "Operador")}, "test")) };
        var create = new CreateDeploymentResourceCommand(); var update = new UpdateDeploymentResourceCommand();
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.Is<CreateDeploymentResourceMediatorCommand>(request => request.Payload == create && request.ActorUserId == InventoryFakerBuilder.Actor && request.ActorName == "Operador"), cancellation.Token)).ReturnsAsync(new CreateDeploymentResourceResult { Conflict = conflict ? "Mudou" : null });
        mediator.Setup(item => item.Send(It.Is<UpdateDeploymentResourceMediatorCommand>(request => request.ExternalId == id && request.Payload == update && request.ActorUserId == InventoryFakerBuilder.Actor), cancellation.Token)).ReturnsAsync(new UpdateDeploymentResourceResult { Conflict = conflict ? "Mudou" : null });
        var controller = new DeploymentResourceController(mediator.Object, new CurrentActor(new HttpContextAccessor { HttpContext = context })) { ControllerContext = new() { HttpContext = context } };
        var created = await controller.Create(create, cancellation.Token); var updated = await controller.Update(id, update, cancellation.Token);
        if (conflict) { Assert.IsType<ConflictObjectResult>(created); Assert.IsType<ConflictObjectResult>(updated); }
        else { Assert.Equal(201, Assert.IsType<ObjectResult>(created).StatusCode); Assert.IsType<OkObjectResult>(updated); }
        mediator.VerifyAll();
    }
}
