using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.ManagedServiceFeature;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Commands;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Queries;
using Owh.Olivaw.Client.Features.ManagedServiceFeature.Commands;
using Owh.Olivaw.Client.Features.ManagedServiceFeature.Queries;
using Owh.Olivaw.Tests.Mocks;
using Xunit;

namespace Owh.Olivaw.Tests.ManagedServiceFeatureTest;
public class ManagedServiceControllerTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Commands_should_use_authenticated_actor_and_translate_conflicts(bool conflict)
    {
        using var cancellation = new CancellationTokenSource();
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", ManagedServiceFakerBuilder.Actor.ToString()), new Claim("name", "Operador") }, "test")) };
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.Is<CreateManagedServiceMediatorCommand>(request => request.ActorUserId == ManagedServiceFakerBuilder.Actor && request.ActorName == "Operador"), cancellation.Token)).ReturnsAsync(new CreateManagedServiceResult { Conflict = conflict ? "Conflito" : null, Response = new() { ExternalId = Guid.NewGuid(), Revision = 1 } });
        mediator.Setup(item => item.Send(It.Is<UpdateManagedServiceMediatorCommand>(request => request.ActorUserId == ManagedServiceFakerBuilder.Actor), cancellation.Token)).ReturnsAsync(new UpdateManagedServiceResult { Conflict = conflict ? "Conflito" : null });
        var controller = new ManagedServiceController(mediator.Object, new CurrentActor(new HttpContextAccessor { HttpContext = context })) { ControllerContext = new ControllerContext { HttpContext = context } };
        var created = await controller.Create(new CreateManagedServiceCommand(), cancellation.Token);
        var updated = await controller.Update(Guid.NewGuid(), new UpdateManagedServiceCommand(), cancellation.Token);
        if (conflict) { Assert.IsType<ConflictObjectResult>(created); Assert.IsType<ConflictObjectResult>(updated); }
        else { Assert.IsType<CreatedAtActionResult>(created); Assert.IsType<OkObjectResult>(updated); }
        mediator.VerifyAll();
    }
    [Fact]
    public async Task Queries_should_forward_public_id_payload_and_token()
    {
        var id = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var mediator = new Mock<IMediator>();
        var payload = new ReadManagedServiceByFilterQuery();
        mediator.Setup(item => item.Send(It.Is<ReadManagedServiceByFilterMediatorQuery>(request => request.Payload == payload), cancellation.Token)).ReturnsAsync(new ReadManagedServiceByFilterResponse());
        mediator.Setup(item => item.Send(It.Is<ReadManagedServiceByIdMediatorQuery>(request => request.Payload.ExternalId == id), cancellation.Token)).ReturnsAsync(new ReadManagedServiceByIdResponse());
        var controller = new ManagedServiceController(mediator.Object, new CurrentActor(new HttpContextAccessor()));
        Assert.IsType<OkObjectResult>(await controller.ReadByFilter(payload, cancellation.Token));
        Assert.IsType<OkObjectResult>(await controller.ReadById(id, cancellation.Token));
        mediator.VerifyAll();
    }
}
