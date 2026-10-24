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
using Owh.Olivaw.Api.Features.UsageReadingFeature;
using Owh.Olivaw.Api.Features.UsageReadingFeature.Commands;
using Owh.Olivaw.Api.Features.UsageReadingFeature.Queries;
using Owh.Olivaw.Client.Features.UsageReadingFeature.Commands;
using Owh.Olivaw.Client.Features.UsageReadingFeature.Queries;
namespace Owh.Olivaw.Tests.UsageReadingFeatureTest;
public sealed class UsageReadingControllerTest
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Create_should_forward_public_payload_actor_and_token_and_translate_conflict(bool conflict)
    {
        using var cancellation = new CancellationTokenSource();
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", InventoryFakerBuilder.Actor.ToString()), new Claim("name", "Operador") }, "test")) };
        var mediator = new Mock<IMediator>(); var payload = new CreateUsageReadingCommand();
        mediator.Setup(item => item.Send(It.Is<CreateUsageReadingMediatorCommand>(request => request.Payload == payload && request.ActorUserId == InventoryFakerBuilder.Actor && request.ActorName == "Operador"), cancellation.Token)).ReturnsAsync(new CreateUsageReadingResult { Conflict = conflict ? "Conflito" : null });
        var controller = new UsageReadingController(mediator.Object, new CurrentActor(new HttpContextAccessor { HttpContext = context })) { ControllerContext = new() { HttpContext = context } };
        var result = await controller.Create(payload, cancellation.Token);
        if (conflict) Assert.IsType<ConflictObjectResult>(result); else Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
        mediator.VerifyAll();
    }
    [Fact]
    public async Task Read_should_forward_query_and_token()
    {
        using var cancellation = new CancellationTokenSource();
        var mediator = new Mock<IMediator>(); var payload = new ReadUsageReadingByFilterQuery();
        mediator.Setup(item => item.Send(It.Is<ReadUsageReadingByFilterMediatorQuery>(request => request.Payload == payload), cancellation.Token)).ReturnsAsync(new ReadUsageReadingByFilterResponse());
        var controller = new UsageReadingController(mediator.Object, new CurrentActor(new HttpContextAccessor()));
        Assert.IsType<OkObjectResult>(await controller.Read(payload, cancellation.Token)); mediator.VerifyAll();
    }
}
