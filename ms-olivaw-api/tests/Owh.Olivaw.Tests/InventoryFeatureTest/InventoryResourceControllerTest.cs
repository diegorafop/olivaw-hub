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
public sealed class InventoryResourceControllerTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Commands_should_forward_payload_actor_and_token_and_translate_conflicts(bool conflict)
    {
        using var cancellation = new CancellationTokenSource(); var id = Guid.NewGuid();
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("sub", InventoryFakerBuilder.Actor.ToString()), new Claim("name", "Operador")}, "test")) };
        var create = new CreateInventoryResourceCommand(); var update = new UpdateInventoryResourceCommand();
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.Is<CreateInventoryResourceMediatorCommand>(request => request.Payload == create && request.ActorUserId == InventoryFakerBuilder.Actor && request.ActorName == "Operador"), cancellation.Token)).ReturnsAsync(new CreateInventoryResourceResult { Conflict = conflict ? "Mudou" : null });
        mediator.Setup(item => item.Send(It.Is<UpdateInventoryResourceMediatorCommand>(request => request.ExternalId == id && request.Payload == update && request.ActorUserId == InventoryFakerBuilder.Actor), cancellation.Token)).ReturnsAsync(new UpdateInventoryResourceResult { Conflict = conflict ? "Mudou" : null });
        var controller = new InventoryResourceController(mediator.Object, new CurrentActor(new HttpContextAccessor { HttpContext = context })) { ControllerContext = new() { HttpContext = context } };
        var created = await controller.Create(create, cancellation.Token); var updated = await controller.Update(id, update, cancellation.Token);
        if (conflict) { Assert.IsType<ConflictObjectResult>(created); Assert.IsType<ConflictObjectResult>(updated); }
        else { Assert.Equal(201, Assert.IsType<CreatedAtActionResult>(created).StatusCode); Assert.IsType<OkObjectResult>(updated); }
        mediator.VerifyAll();
    }
    [Fact]
    public async Task Queries_should_forward_public_id_and_filter()
    {
        var id = Guid.NewGuid(); var payload = new ReadInventoryResourceByFilterQuery(); var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.Is<ReadInventoryResourceByFilterMediatorQuery>(request => request.Payload == payload), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadInventoryResourceByFilterResponse());
        mediator.Setup(item => item.Send(It.Is<ReadInventoryResourceByIdMediatorQuery>(request => request.Payload.ExternalId == id), It.IsAny<CancellationToken>())).ReturnsAsync(new ReadInventoryResourceByIdResponse());
        var controller = new InventoryResourceController(mediator.Object, new CurrentActor(new HttpContextAccessor()));
        Assert.IsType<OkObjectResult>(await controller.ReadByFilter(payload, CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.ReadById(id, CancellationToken.None)); mediator.VerifyAll();
    }
}
