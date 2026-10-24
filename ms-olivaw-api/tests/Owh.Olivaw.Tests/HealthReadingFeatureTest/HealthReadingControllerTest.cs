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
using Owh.Olivaw.Api.Features.HealthReadingFeature;
using Owh.Olivaw.Api.Features.HealthReadingFeature.Commands;
using Owh.Olivaw.Api.Features.HealthReadingFeature.Queries;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Commands;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Queries;
namespace Owh.Olivaw.Tests.HealthReadingFeatureTest;
public sealed class HealthReadingControllerTest
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Create_should_forward_public_payload_actor_and_token_and_translate_conflict(bool conflict)
    {
        using var cancellation = new CancellationTokenSource();
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", InventoryFakerBuilder.Actor.ToString()), new Claim("name", "Operador") }, "test")) };
        var mediator = new Mock<IMediator>(); var payload = new CreateHealthReadingCommand();
        mediator.Setup(item => item.Send(It.Is<CreateHealthReadingMediatorCommand>(request => request.Payload == payload && request.ActorUserId == InventoryFakerBuilder.Actor && request.ActorName == "Operador"), cancellation.Token)).ReturnsAsync(new CreateHealthReadingResult { Conflict = conflict ? "Conflito" : null });
        var controller = new HealthReadingController(mediator.Object, new CurrentActor(new HttpContextAccessor { HttpContext = context })) { ControllerContext = new() { HttpContext = context } };
        var result = await controller.Create(payload, cancellation.Token);
        if (conflict) Assert.IsType<ConflictObjectResult>(result); else Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
        mediator.VerifyAll();
    }
    [Fact]
    public async Task Read_should_forward_query_and_token()
    {
        using var cancellation = new CancellationTokenSource();
        var mediator = new Mock<IMediator>(); var payload = new ReadHealthReadingByFilterQuery();
        mediator.Setup(item => item.Send(It.Is<ReadHealthReadingByFilterMediatorQuery>(request => request.Payload == payload), cancellation.Token)).ReturnsAsync(new ReadHealthReadingByFilterResponse());
        var controller = new HealthReadingController(mediator.Object, new CurrentActor(new HttpContextAccessor()));
        Assert.IsType<OkObjectResult>(await controller.Read(payload, cancellation.Token)); mediator.VerifyAll();
    }
}
